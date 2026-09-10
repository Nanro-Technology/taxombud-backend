using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Chats.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Notifications.DTOs;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Communications;
using Microsoft.Extensions.Logging;

namespace TaxOmbud.Application.Services;

public class CaseDiscussionService : ICaseDiscussionService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<CaseDiscussionService> _logger;

    public CaseDiscussionService(
        IApplicationDbContext context,
        INotificationsService notificationsService,
        ILogger<CaseDiscussionService> logger)
    {
        _context = context;
        _notificationsService = notificationsService;
        _logger = logger;
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private async Task<Domain.Entities.Cases.Case?> ResolveCaseAsync(Guid caseOrComplaintId, CancellationToken ct = default)
    {
        return await _context.Cases
            .Include(c => c.ActiveWorkflowInstance)
                .ThenInclude(wi => wi!.Workflow)
                    .ThenInclude(w => w.Levels)
                        .ThenInclude(l => l.Targets)   // multi-target junction
            .Include(c => c.ActiveWorkflowInstance)
                .ThenInclude(wi => wi!.InstanceLevels)
                    .ThenInclude(il => il.AssignedRole)
            .Include(c => c.ActiveWorkflowInstance)
                .ThenInclude(wi => wi!.InstanceLevels)
                    .ThenInclude(il => il.WorkflowLevel)
                        .ThenInclude(wl => wl.Targets)  // multi-target junction
            .FirstOrDefaultAsync(c => c.Id == caseOrComplaintId || c.ComplaintId == caseOrComplaintId, ct);
    }

    private async Task<(Guid? RoleId, string? RoleName)> ResolveInvestigationRoleAsync(Domain.Entities.Cases.Case? caseItem, CancellationToken ct = default)
    {
        if (caseItem?.ActiveWorkflowInstance != null)
        {
            var levels = caseItem.ActiveWorkflowInstance.InstanceLevels;
            if (levels != null && levels.Any())
            {
                // Prefer instance level with AssignedRoleId already set, or whose WorkflowLevel has Investigation LevelRole
                var invLevel = levels.FirstOrDefault(l =>
                    (l.WorkflowLevel != null && l.WorkflowLevel.LevelRole == TaxOmbud.Domain.Enums.LevelRole.Investigation) ||
                    l.WorkflowLevel?.Name.Contains("investigation", StringComparison.OrdinalIgnoreCase) == true
                ) ?? levels.FirstOrDefault(l => l.AssignedRoleId.HasValue);

                if (invLevel != null)
                {
                    var rId = invLevel.AssignedRoleId;
                    var rName = invLevel.AssignedRole?.Name;
                    if (rId.HasValue)
                    {
                        if (string.IsNullOrEmpty(rName))
                        {
                            var role = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Id == rId.Value, ct);
                            rName = role?.Name;
                        }
                        return (rId, rName);
                    }
                }
            }

            if (caseItem.ActiveWorkflowInstance.Workflow?.Levels != null)
            {
                var wfLevels = caseItem.ActiveWorkflowInstance.Workflow.Levels;
                // Find level with Investigation LevelRole, or fall back to any level with Role targets
                var invWfLevel = wfLevels.FirstOrDefault(l =>
                    l.LevelRole == TaxOmbud.Domain.Enums.LevelRole.Investigation ||
                    l.Name.Contains("investigation", StringComparison.OrdinalIgnoreCase)
                ) ?? wfLevels.FirstOrDefault(l => l.Targets.Any(t => t.TargetType == TaxOmbud.Domain.Enums.WorkflowLevelTargetType.Role));

                if (invWfLevel != null)
                {
                    var roleTarget = invWfLevel.Targets.FirstOrDefault(t => t.TargetType == TaxOmbud.Domain.Enums.WorkflowLevelTargetType.Role);
                    if (roleTarget != null)
                    {
                        var role = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Id == roleTarget.TargetId, ct);
                        return (role?.Id, role?.Name);
                    }
                }
            }
        }

        // Fallback: check default active workflow in database
        var defaultWf = await _context.Workflows
            .Include(w => w.Levels)
                .ThenInclude(l => l.Targets)
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.IsDefault)
            .FirstOrDefaultAsync(ct);

        if (defaultWf?.Levels != null)
        {
            var invLvl = defaultWf.Levels.FirstOrDefault(l =>
                l.LevelRole == TaxOmbud.Domain.Enums.LevelRole.Investigation ||
                l.Name.Contains("investigation", StringComparison.OrdinalIgnoreCase)
            ) ?? defaultWf.Levels.FirstOrDefault(l => l.Targets.Any(t => t.TargetType == TaxOmbud.Domain.Enums.WorkflowLevelTargetType.Role));

            if (invLvl != null)
            {
                var roleTarget = invLvl.Targets.FirstOrDefault(t => t.TargetType == TaxOmbud.Domain.Enums.WorkflowLevelTargetType.Role);
                if (roleTarget != null)
                {
                    var role = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Id == roleTarget.TargetId, ct);
                    return (role?.Id, role?.Name);
                }
            }
        }

        return (null, null);
    }

    // ── Thread Management ─────────────────────────────────────────────────────

    public async Task<Guid> OpenDiscussionThreadAsync(Guid caseId, CancellationToken ct = default)
    {
        var caseItem = await ResolveCaseAsync(caseId, ct);

        if (caseItem == null)
            throw new InvalidOperationException($"Case {caseId} not found.");

        var canonicalCaseId = caseItem.Id;

        // Idempotent — if a thread already exists for this case at Stage 5, return it
        var existing = await _context.AgentChats
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == caseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (existing != null) return existing.Id;

        if (caseItem.CurrentStage != WorkflowStage.InvestigationAndResolution)
            throw new InvalidOperationException($"Discussion threads are only available during Stage 5 — Investigation & Resolution. The case is currently at stage '{caseItem.CurrentStage}'.");

        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        var thread = new AgentChat
        {
            Id = Guid.NewGuid(),
            Topic = $"Investigation Discussion — {caseRef}",
            IsGroupChat = true,
            CaseId = canonicalCaseId,
            BoundToStage = WorkflowStage.InvestigationAndResolution,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.AgentChats.Add(thread);

        // Seed Tier 1 participants from the Stage 5 workflow role assignment
        var stage5Participants = await SeedTier1ParticipantsAsync(thread, caseItem, ct);

        // Post a system message to open the thread
        var systemMsg = new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = thread.Id,
            SenderId = Guid.Empty,
            Content = $"Investigation Discussion Thread opened for Case {caseRef}. All assigned officers may post their findings and conclusions here.",
            MessageType = "system",
            CreatedAt = DateTime.UtcNow
        };
        _context.AgentChatMessages.Add(systemMsg);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Case discussion thread {ThreadId} opened for Case {CaseRef} with {Count} Tier 1 participants.",
            thread.Id, caseRef, stage5Participants);

        return thread.Id;
    }

    private async Task<int> SeedTier1ParticipantsAsync(AgentChat thread, Domain.Entities.Cases.Case caseItem, CancellationToken ct)
    {
        int count = 0;
        var (stage5RoleId, stage5RoleName) = await ResolveInvestigationRoleAsync(caseItem, ct);

        if (stage5RoleId.HasValue)
        {
            // All active users assigned to this custom Role are Tier 1 participants
            var roleUsers = await _context.Users
                .Where(u => u.RoleId == stage5RoleId.Value && u.Status == Domain.Enums.UserStatus.Active)
                .Select(u => u.Id)
                .ToListAsync(ct);

            foreach (var userId in roleUsers)
            {
                bool exists = await _context.AgentChatParticipants
                    .AnyAsync(p => p.AgentChatId == thread.Id && p.UserId == userId, ct);

                if (!exists)
                {
                    _context.AgentChatParticipants.Add(new AgentChatParticipant
                    {
                        Id = Guid.NewGuid(),
                        AgentChatId = thread.Id,
                        UserId = userId,
                        RoleName = stage5RoleName,
                        ParticipantTier = "role",
                        IsReadOnly = false,
                        JoinedAt = DateTimeOffset.UtcNow
                    });
                    count++;
                }
            }
        }

        // Always include the assigned officer as a Tier 1 participant if not already captured
        if (caseItem.AssignedOfficerId.HasValue)
        {
            var officer = await _context.OfficerProfiles.FirstOrDefaultAsync(o => o.Id == caseItem.AssignedOfficerId.Value, ct);
            var officerUserId = officer?.UserId ?? caseItem.AssignedOfficerId.Value;

            bool alreadyAdded = count > 0 && await _context.AgentChatParticipants
                .AnyAsync(p => p.AgentChatId == thread.Id && p.UserId == officerUserId, ct);

            if (!alreadyAdded)
            {
                _context.AgentChatParticipants.Add(new AgentChatParticipant
                {
                    Id = Guid.NewGuid(),
                    AgentChatId = thread.Id,
                    UserId = officerUserId,
                    RoleName = stage5RoleName ?? "Assigned Officer",
                    ParticipantTier = "role",
                    IsReadOnly = false,
                    JoinedAt = DateTimeOffset.UtcNow
                });
                count++;
            }
        }

        // Always include SuperAdmins / Administrators as Tier 1 oversight participants
        var admins = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.UserType == Domain.Enums.UserType.StaffUser && 
                        u.Status == Domain.Enums.UserStatus.Active &&
                        (u.Email.Contains("admin") || (u.Role != null && (u.Role.Name.Contains("Admin") || u.Role.Name.Contains("Super")))))
            .ToListAsync(ct);

        foreach (var admin in admins)
        {
            bool adminAlreadyAdded = count > 0 && await _context.AgentChatParticipants
                .AnyAsync(p => p.AgentChatId == thread.Id && p.UserId == admin.Id, ct);

            if (!adminAlreadyAdded)
            {
                _context.AgentChatParticipants.Add(new AgentChatParticipant
                {
                    Id = Guid.NewGuid(),
                    AgentChatId = thread.Id,
                    UserId = admin.Id,
                    RoleName = admin.Role?.Name ?? "Super Administrator",
                    ParticipantTier = "admin",
                    IsReadOnly = false,
                    JoinedAt = DateTimeOffset.UtcNow
                });
                count++;
            }
        }

        // Also include all officers who worked on previous stages of this case
        var canonicalCaseId = caseItem.Id;
        var priorAuditUserIds = await _context.CaseWorkflowAuditLogs
            .Where(l => (l.CaseId == canonicalCaseId || l.CaseId == caseItem.ComplaintId) && l.PerformedByUserId != Guid.Empty)
            .Select(l => l.PerformedByUserId)
            .Distinct()
            .ToListAsync(ct);

        var priorTaskUserIds = await _context.CaseApprovalTasks
            .Where(t => (t.CaseId == canonicalCaseId || t.CaseId == caseItem.ComplaintId) && t.AssignedUserId.HasValue)
            .Select(t => t.AssignedUserId!.Value)
            .Distinct()
            .ToListAsync(ct);

        var allPriorOfficers = priorAuditUserIds.Concat(priorTaskUserIds).Distinct().ToList();
        foreach (var priorUserId in allPriorOfficers)
        {
            bool exists = await _context.AgentChatParticipants
                .AnyAsync(p => p.AgentChatId == thread.Id && p.UserId == priorUserId, ct);

            if (!exists)
            {
                var priorUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == priorUserId, ct);
                _context.AgentChatParticipants.Add(new AgentChatParticipant
                {
                    Id = Guid.NewGuid(),
                    AgentChatId = thread.Id,
                    UserId = priorUserId,
                    RoleName = priorUser?.Role?.Name ?? "Case Officer",
                    ParticipantTier = "contributor",
                    IsReadOnly = false,
                    JoinedAt = DateTimeOffset.UtcNow
                });
                count++;
            }
        }

        return count;
    }

    // ── Get Thread ────────────────────────────────────────────────────────────

    private static bool IsSuperAdmin(Domain.Entities.Identity.User? user)
    {
        if (user == null) return false;
        if (user.UserType != Domain.Enums.UserType.StaffUser) return false;

        var roleName = user.Role?.Name ?? string.Empty;
        var email = user.Email ?? string.Empty;

        return string.Equals(email, "admin@taxombud.gov.ng", StringComparison.OrdinalIgnoreCase) ||
               email.Contains("admin", StringComparison.OrdinalIgnoreCase) ||
               roleName.Contains("Super Admin", StringComparison.OrdinalIgnoreCase) ||
               roleName.Contains("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               roleName.Contains("Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool UserHasWorkflowRole(Domain.Entities.Identity.User? user, Guid? stageRoleId, string? stageRoleName)
    {
        if (user == null) return false;

        var userRoleId = user.RoleId ?? user.Role?.Id;
        if (stageRoleId.HasValue && userRoleId.HasValue && userRoleId.Value == stageRoleId.Value)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(stageRoleName) && !string.IsNullOrWhiteSpace(user.Role?.Name))
        {
            return string.Equals(user.Role.Name.Trim(), stageRoleName.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private async Task<bool> IsAssignedOfficerAsync(Domain.Entities.Cases.Case? caseItem, Guid userId, CancellationToken ct)
    {
        if (caseItem == null || !caseItem.AssignedOfficerId.HasValue) return false;
        if (caseItem.AssignedOfficerId.Value == userId) return true;

        return await _context.OfficerProfiles.AnyAsync(
            o => o.Id == caseItem.AssignedOfficerId.Value && o.UserId == userId, ct);
    }

    private async Task<bool> HasWorkedOnCaseAsync(Guid caseId, Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty) return false;

        var caseItem = await _context.Cases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId, ct);
        var canonicalCaseId = caseItem?.Id ?? caseId;

        // Check if directly assigned officer
        if (caseItem?.AssignedOfficerId.HasValue == true)
        {
            if (caseItem.AssignedOfficerId.Value == userId) return true;
            var officer = await _context.OfficerProfiles.AsNoTracking().FirstOrDefaultAsync(o => o.Id == caseItem.AssignedOfficerId.Value, ct);
            if (officer != null && officer.UserId == userId) return true;
        }

        // Check if performed any workflow audit action on this case
        bool performedAudit = await _context.CaseWorkflowAuditLogs.AsNoTracking()
            .AnyAsync(l => (l.CaseId == canonicalCaseId || (caseItem != null && l.CaseId == caseItem.ComplaintId)) && l.PerformedByUserId == userId, ct);
        if (performedAudit) return true;

        // Check if assigned to any approval task on this case
        bool hasApprovalTask = await _context.CaseApprovalTasks.AsNoTracking()
            .AnyAsync(t => (t.CaseId == canonicalCaseId || (caseItem != null && t.CaseId == caseItem.ComplaintId)) && t.AssignedUserId.HasValue && t.AssignedUserId.Value == userId, ct);
        if (hasApprovalTask) return true;

        // Check case status history or communications
        bool inStatusHistory = await _context.CaseStatusHistories.AsNoTracking()
            .AnyAsync(sh => (sh.CaseId == canonicalCaseId || (caseItem != null && sh.CaseId == caseItem.ComplaintId)) && sh.ChangedByUserId == userId, ct);
        if (inStatusHistory) return true;

        return false;
    }

    public async Task<CaseDiscussionThreadDto?> GetDiscussionThreadAsync(Guid caseId, Guid? currentUserId = null, CancellationToken ct = default)
    {
        var caseItem = await ResolveCaseAsync(caseId, ct);
        var canonicalCaseId = caseItem?.Id ?? caseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == caseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return null;

        var (stage5RoleId, stage5RoleName) = await ResolveInvestigationRoleAsync(caseItem, ct);

        bool canCurrentUserPost = false;
        if (currentUserId.HasValue)
        {
            var currentUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);
            if (currentUser != null)
            {
                bool hasWfRole = UserHasWorkflowRole(currentUser, stage5RoleId, stage5RoleName);
                bool isAssigned = await IsAssignedOfficerAsync(caseItem, currentUserId.Value, ct);
                bool isSuper = IsSuperAdmin(currentUser);
                bool hasWorkedOnCase = await HasWorkedOnCaseAsync(canonicalCaseId, currentUserId.Value, ct);

                // Officers assigned to stage role, assigned case officers, superadmins, or any officer who worked on the case can post
                canCurrentUserPost = hasWfRole || isAssigned || isSuper || hasWorkedOnCase;
            }
        }

        // Query all users who worked on the case so they are included as contributors
        var priorAuditUserIds = await _context.CaseWorkflowAuditLogs
            .Where(l => (l.CaseId == canonicalCaseId || (caseItem != null && l.CaseId == caseItem.ComplaintId)) && l.PerformedByUserId != Guid.Empty)
            .Select(l => l.PerformedByUserId)
            .Distinct()
            .ToListAsync(ct);

        var priorTaskUserIds = await _context.CaseApprovalTasks
            .Where(t => (t.CaseId == canonicalCaseId || (caseItem != null && t.CaseId == caseItem.ComplaintId)) && t.AssignedUserId.HasValue)
            .Select(t => t.AssignedUserId!.Value)
            .Distinct()
            .ToListAsync(ct);

        var contributorUserIds = priorAuditUserIds.Concat(priorTaskUserIds).ToHashSet();

        // Resolve user details for participants
        var participantUserIds = thread.Participants.Select(p => p.UserId).ToHashSet();
        foreach (var id in contributorUserIds)
        {
            participantUserIds.Add(id);
        }

        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => participantUserIds.Contains(u.Id))
            .ToListAsync(ct);

        var participants = thread.Participants.Select(p =>
        {
            var user = users.FirstOrDefault(u => u.Id == p.UserId);
            bool isSuper = IsSuperAdmin(user);
            bool hasWf = UserHasWorkflowRole(user, stage5RoleId, stage5RoleName);
            bool isAssigned = caseItem != null && caseItem.AssignedOfficerId.HasValue && caseItem.AssignedOfficerId.Value == p.UserId;
            bool hasWorked = contributorUserIds.Contains(p.UserId);

            bool userCanPost = isSuper || hasWf || isAssigned || hasWorked;

            return new DiscussionParticipantDto
            {
                UserId = p.UserId,
                FullName = user != null ? user.FullName : p.UserId.ToString(),
                AvatarUrl = null, // No avatar field on User entity currently
                RoleName = user?.Role?.Name ?? p.RoleName,
                ParticipantTier = hasWorked && !hasWf && !isSuper ? "contributor" : p.ParticipantTier,
                IsReadOnly = !userCanPost,
                IsOnline = false // Resolved by SignalR ChatHub presence in real-time
            };
        }).ToList();

        // Add any prior contributor not yet recorded in thread.Participants
        var recordedParticipantIds = thread.Participants.Select(p => p.UserId).ToHashSet();
        foreach (var contributorId in contributorUserIds.Where(id => !recordedParticipantIds.Contains(id)))
        {
            var user = users.FirstOrDefault(u => u.Id == contributorId);
            if (user != null)
            {
                participants.Add(new DiscussionParticipantDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    AvatarUrl = null,
                    RoleName = user.Role?.Name ?? "Case Contributor",
                    ParticipantTier = "contributor",
                    IsReadOnly = false,
                    IsOnline = false
                });
            }
        }

        // Resolve sender details for messages
        var senderIds = thread.Messages.Select(m => m.SenderId).Distinct().ToList();
        var senders = await _context.Users
            .Include(u => u.Role)
            .Where(u => senderIds.Contains(u.Id))
            .ToListAsync(ct);

        var messages = thread.Messages.Select(m =>
        {
            var sender = senders.FirstOrDefault(u => u.Id == m.SenderId);
            var receipts = JsonSerializer.Deserialize<List<ReadReceiptDto>>(m.ReadReceipts) ?? new();
            return new DiscussionMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = sender != null ? sender.FullName : (m.SenderId == Guid.Empty ? "System" : m.SenderId.ToString()),
                SenderAvatar = null,
                SenderRole = sender?.Role?.Name,
                Content = m.Content,
                MessageType = m.MessageType,
                IsPinned = m.IsPinned,
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                CreatedAt = m.CreatedAt,
                ReadReceipts = receipts
            };
        }).ToList();

        var caseRef = thread.Case?.CaseNumber?.Value ?? thread.CaseId?.ToString() ?? string.Empty;

        return new CaseDiscussionThreadDto
        {
            ChatId = thread.Id,
            CaseId = caseId,
            CaseNumber = caseRef,
            IsLocked = thread.IsLocked,
            LockedAt = thread.LockedAt,
            RequiredRoleName = stage5RoleName,
            RequiredRoleId = stage5RoleId,
            CanCurrentUserPost = canCurrentUserPost,
            Participants = participants,
            Messages = messages
        };
    }

    // ── Messaging ─────────────────────────────────────────────────────────────

    public async Task<DiscussionMessageDto?> SendMessageAsync(SendDiscussionMessageCommand cmd, Guid senderId, CancellationToken ct = default)
    {
        var caseItem = await ResolveCaseAsync(cmd.CaseId, ct);
        var canonicalCaseId = caseItem?.Id ?? cmd.CaseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == cmd.CaseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return null;

        var sender = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == senderId, ct);

        if (sender == null) return null;

        var isSuperAdmin = IsSuperAdmin(sender);
        var (stage5RoleId, stage5RoleName) = await ResolveInvestigationRoleAsync(caseItem, ct);

        // Check if the user has the workflow-assigned role for this stage
        bool hasWorkflowRole = UserHasWorkflowRole(sender, stage5RoleId, stage5RoleName);

        // Check if the user is the assigned case officer
        bool isAssignedOfficer = await IsAssignedOfficerAsync(caseItem, senderId, ct);

        // Check if the user has worked on this case in any prior or current stage
        bool hasWorkedOnCase = await HasWorkedOnCaseAsync(canonicalCaseId, senderId, ct);

        // RESTRICTION: Users with the assigned workflow role, assigned officer,
        // SuperAdmin oversight, OR officers who have worked on the case can chat.
        bool isAuthorized = hasWorkflowRole || isAssignedOfficer || isSuperAdmin || hasWorkedOnCase;

        if (!isAuthorized)
        {
            var currentRole = sender.Role?.Name ?? "Unassigned";
            throw new UnauthorizedAccessException(
                $"Access restricted. Only officers who have worked on this case, designated investigation officers ({stage5RoleName ?? "Investigation Role"}), or administrators can participate in this thread. Your current role is '{currentRole}'."
            );
        }

        // Thread lock check: SuperAdmin can post for oversight, regular users cannot post on locked threads
        if (thread.IsLocked && !isSuperAdmin)
        {
            throw new InvalidOperationException("This investigation discussion thread is concluded and locked. New entries are disabled.");
        }

        // If authorized but not yet in participant list, auto-enroll them
        var participant = thread.Participants.FirstOrDefault(p => p.UserId == senderId);
        if (participant == null)
        {
            participant = new AgentChatParticipant
            {
                Id = Guid.NewGuid(),
                AgentChatId = thread.Id,
                UserId = senderId,
                RoleName = sender.Role?.Name ?? stage5RoleName ?? "Case Contributor",
                ParticipantTier = hasWorkflowRole ? "role" : (hasWorkedOnCase ? "contributor" : (isSuperAdmin ? "admin" : "individual")),
                IsReadOnly = false,
                JoinedAt = DateTimeOffset.UtcNow
            };
            _context.AgentChatParticipants.Add(participant);
            thread.Participants.Add(participant);
            await _context.SaveChangesAsync(ct);
        }
        else if (participant.IsReadOnly && !isSuperAdmin && !hasWorkflowRole && !isAssignedOfficer && !hasWorkedOnCase)
        {
            throw new UnauthorizedAccessException("You have read-only access to this discussion thread and cannot post messages.");
        }

        var message = new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = thread.Id,
            SenderId = senderId,
            Content = cmd.Content,
            MessageType = cmd.MessageType ?? "message",
            AttachmentUrl = cmd.AttachmentUrl,
            AttachmentFileName = cmd.AttachmentFileName,
            IsPinned = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.AgentChatMessages.Add(message);
        await _context.SaveChangesAsync(ct);

        // If this is a CONCLUSION message, notify the supervisor (Stage 6 role)
        if (message.MessageType == "conclusion")
        {
            await NotifySupervisorOfConclusionAsync(canonicalCaseId, senderId, cmd.Content, ct);
        }

        return new DiscussionMessageDto
        {
            Id = message.Id,
            SenderId = senderId,
            SenderName = sender?.FullName ?? senderId.ToString(),
            SenderAvatar = null,
            SenderRole = sender?.Role?.Name,
            Content = message.Content,
            MessageType = message.MessageType,
            IsPinned = false,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentFileName = message.AttachmentFileName,
            CreatedAt = message.CreatedAt,
            ReadReceipts = new()
        };
    }

    private async Task NotifySupervisorOfConclusionAsync(Guid caseId, Guid concludedByUserId, string conclusionText, CancellationToken ct)
    {
        try
        {
            var caseItem = await _context.Cases
                .Include(c => c.ActiveWorkflowInstance)
                    .ThenInclude(wi => wi!.InstanceLevels)
                        .ThenInclude(il => il.AssignedRole)
                .FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId, ct);

            if (caseItem?.ActiveWorkflowInstance == null) return;

            // Resolve Stage 6 role — the last instance level that has an assigned role
            var stage6Level = caseItem.ActiveWorkflowInstance.InstanceLevels
                .OrderByDescending(l => l.LevelNumber)
                .FirstOrDefault(l => l.AssignedRoleId.HasValue && l.LevelNumber > 5);

            if (stage6Level?.AssignedRoleId == null) return;

            var supervisors = await _context.Users
                .Where(u => u.RoleId == stage6Level.AssignedRoleId.Value && u.Status == Domain.Enums.UserStatus.Active)
                .ToListAsync(ct);

            var concludedBy = await _context.Users
                .Where(u => u.Id == concludedByUserId)
                .FirstOrDefaultAsync(ct);

            var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();
            var officerName = concludedBy?.FullName ?? "An officer";

            foreach (var supervisor in supervisors)
            {
                await _notificationsService.SendNotificationAsync(new SendNotificationCommand(
                    supervisor.Id,
                    $"Investigation Conclusion — Case {caseRef}",
                    $"{officerName} has posted a formal CONCLUSION in the investigation discussion for Case {caseRef}. Please review and proceed to Stage 6."
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify supervisor of conclusion for Case {CaseId}", caseId);
        }
    }

    // ── Pin Message ───────────────────────────────────────────────────────────

    public async Task<bool> PinMessageAsync(PinDiscussionMessageCommand cmd, Guid requestedBy, CancellationToken ct = default)
    {
        var message = await _context.AgentChatMessages.FindAsync(new object[] { cmd.MessageId }, ct);
        if (message == null) return false;

        message.IsPinned = cmd.IsPinned;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Participant Management ────────────────────────────────────────────────

    public async Task<bool> AddRoleToThreadAsync(AddRoleToDiscussionCommand cmd, Guid requestedBy, CancellationToken ct = default)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == cmd.CaseId || c.ComplaintId == cmd.CaseId, ct);
        var canonicalCaseId = caseItem?.Id ?? cmd.CaseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == cmd.CaseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return false;

        // Find the custom Role by name
        var role = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == cmd.RoleName, ct);
        if (role == null) return false;

        var roleUsers = await _context.Users
            .Where(u => u.RoleId == role.Id && u.Status == Domain.Enums.UserStatus.Active)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var existingUserIds = thread.Participants.Select(p => p.UserId).ToHashSet();
        int added = 0;

        foreach (var userId in roleUsers.Where(id => !existingUserIds.Contains(id)))
        {
            _context.AgentChatParticipants.Add(new AgentChatParticipant
            {
                Id = Guid.NewGuid(),
                AgentChatId = thread.Id,
                UserId = userId,
                RoleName = cmd.RoleName,
                ParticipantTier = "individual",
                IsReadOnly = thread.IsLocked,
                JoinedAt = DateTimeOffset.UtcNow
            });
            added++;
        }

        if (added > 0)
        {
            _context.AgentChatMessages.Add(new AgentChatMessage
            {
                Id = Guid.NewGuid(),
                AgentChatId = thread.Id,
                SenderId = Guid.Empty,
                Content = $"All members of role '{cmd.RoleName}' ({added} officer(s)) were added to this discussion by an administrator.",
                MessageType = "system",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);
        }

        return true;
    }

    public async Task<bool> AddIndividualToThreadAsync(AddIndividualParticipantCommand cmd, Guid requestedBy, CancellationToken ct = default)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == cmd.CaseId || c.ComplaintId == cmd.CaseId, ct);
        var canonicalCaseId = caseItem?.Id ?? cmd.CaseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == cmd.CaseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return false;
        if (thread.Participants.Any(p => p.UserId == cmd.UserId)) return true;

        _context.AgentChatParticipants.Add(new AgentChatParticipant
        {
            Id = Guid.NewGuid(),
            AgentChatId = thread.Id,
            UserId = cmd.UserId,
            RoleName = null,
            ParticipantTier = "individual",
            IsReadOnly = thread.IsLocked,
            JoinedAt = DateTimeOffset.UtcNow
        });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct);
        _context.AgentChatMessages.Add(new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = thread.Id,
            SenderId = Guid.Empty,
            Content = $"{(user?.FullName ?? cmd.UserId.ToString())} was individually added to this discussion by an administrator.",
            MessageType = "system",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(RemoveDiscussionParticipantCommand cmd, Guid requestedBy, CancellationToken ct = default)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == cmd.CaseId || c.ComplaintId == cmd.CaseId, ct);
        var canonicalCaseId = caseItem?.Id ?? cmd.CaseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == cmd.CaseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return false;

        var participant = thread.Participants.FirstOrDefault(p => p.UserId == cmd.UserId);
        if (participant == null) return false;

        if (participant.ParticipantTier == "role")
            throw new InvalidOperationException("Role-seeded Tier 1 participants cannot be individually removed.");

        _context.AgentChatParticipants.Remove(participant);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Lock Thread ───────────────────────────────────────────────────────────

    public async Task<bool> LockDiscussionThreadAsync(Guid caseId, CancellationToken ct = default)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId, ct);
        var canonicalCaseId = caseItem?.Id ?? caseId;

        var thread = await _context.AgentChats
            .Include(c => c.Participants)
            .Include(c => c.Case)
                .ThenInclude(c => c!.ActiveWorkflowInstance)
                    .ThenInclude(wi => wi!.InstanceLevels)
                        .ThenInclude(il => il.AssignedRole)
            .FirstOrDefaultAsync(c => (c.CaseId == canonicalCaseId || c.CaseId == caseId) && c.BoundToStage == WorkflowStage.InvestigationAndResolution, ct);

        if (thread == null) return false;
        if (thread.IsLocked) return true;

        thread.IsLocked = true;
        thread.LockedAt = DateTimeOffset.UtcNow;

        // Set all existing participants to read-only
        foreach (var p in thread.Participants.Where(p => !p.IsReadOnly))
            p.IsReadOnly = true;

        // Add Stage 6 role members as read-only
        var stage6Level = thread.Case?.ActiveWorkflowInstance?.InstanceLevels
            .OrderByDescending(l => l.LevelNumber)
            .FirstOrDefault(l => l.AssignedRoleId.HasValue);

        if (stage6Level?.AssignedRoleId.HasValue == true)
        {
            var existingUserIds = thread.Participants.Select(p => p.UserId).ToHashSet();
            var roleName = stage6Level.AssignedRole?.Name;

            var stage6Users = await _context.Users
                .Where(u => u.RoleId == stage6Level.AssignedRoleId.Value && u.Status == Domain.Enums.UserStatus.Active)
                .Select(u => u.Id)
                .ToListAsync(ct);

            foreach (var userId in stage6Users.Where(id => !existingUserIds.Contains(id)))
            {
                _context.AgentChatParticipants.Add(new AgentChatParticipant
                {
                    Id = Guid.NewGuid(),
                    AgentChatId = thread.Id,
                    UserId = userId,
                    RoleName = roleName,
                    ParticipantTier = "stage6",
                    IsReadOnly = true,
                    JoinedAt = DateTimeOffset.UtcNow
                });
            }
        }

        _context.AgentChatMessages.Add(new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = thread.Id,
            SenderId = Guid.Empty,
            Content = "This investigation discussion thread has been locked. The case is advancing to Stage 6 (Decision & Communication). No further posts are permitted. Stage 6 officers may view this thread for reference.",
            MessageType = "system",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Discussion thread {ThreadId} locked for Case {CaseId}.", thread.Id, caseId);

        return true;
    }
}
