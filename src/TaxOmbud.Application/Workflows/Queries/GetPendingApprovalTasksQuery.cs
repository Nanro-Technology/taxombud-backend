using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetPendingApprovalTasksQuery() : IRequest<List<CaseApprovalTaskDto>>;

public class GetPendingApprovalTasksQueryHandler : IRequestHandler<GetPendingApprovalTasksQuery, List<CaseApprovalTaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GetPendingApprovalTasksQueryHandler> _logger;

    public GetPendingApprovalTasksQueryHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ILogger<GetPendingApprovalTasksQueryHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<CaseApprovalTaskDto>> Handle(GetPendingApprovalTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            // 1. Fetch current user with role and department details (read-only)
            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

            var userRoleId = user?.RoleId;
            var userRoleName = user?.Role?.Name ?? string.Empty;

            var isSuperAdmin = (_currentUser != null && (_currentUser.IsInRole("Super Admin") || _currentUser.IsInRole("Admin")))
                            || userRoleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)
                            || userRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(user?.Email, "admin@taxombud.gov.ng", StringComparison.OrdinalIgnoreCase)
                            || (user != null && user.Email.Contains("admin", StringComparison.OrdinalIgnoreCase));

            // 2. Load active workflow template with levels and targets (read-only)
            var activeWorkflow = await _context.Workflows
                .Include(w => w.Levels).ThenInclude(l => l.Targets)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive && !w.IsDeleted, cancellationToken)
                ?? await _context.Workflows
                .Include(w => w.Levels).ThenInclude(l => l.Targets)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IsActive && !w.IsDeleted, cancellationToken);

            // 3. Determine which workflow levels the current user is eligible to act upon
            var eligibleLevelNumbers = new HashSet<int>();
            if (activeWorkflow?.Levels != null && user != null)
            {
                foreach (var lvl in activeWorkflow.Levels)
                {
                    if (IsUserEligibleForLevel(user, lvl))
                    {
                        eligibleLevelNumbers.Add(lvl.LevelNumber);
                    }
                }
            }

            // Quick exit: If non-super admin has no eligible workflow levels and no direct user assignment, return empty list instantly
            // (e.g. an Auditor, HR, or Finance user who is not targeted by any workflow lane)
            var hasDirectAssignments = false;
            if (!isSuperAdmin)
            {
                hasDirectAssignments = await _context.CaseApprovalTasks
                    .AsNoTracking()
                    .AnyAsync(t => t.TaskStatus == WorkflowLevelStatus.Pending
                                && (t.AssignedUserId == currentUserId || (userRoleId.HasValue && t.AssignedRoleId == userRoleId.Value)), cancellationToken)
                    || await _context.Cases
                    .AsNoTracking()
                    .AnyAsync(c => c.Status != CaseStatus.Closed && c.AssignedOfficer != null && c.AssignedOfficer.UserId == currentUserId, cancellationToken);

                if (!hasDirectAssignments && eligibleLevelNumbers.Count == 0)
                {
                    return new List<CaseApprovalTaskDto>();
                }
            }

            // 4. Query recorded pending approval tasks with targeted filtering
            var recordedTasksQuery = _context.CaseApprovalTasks
                .Include(t => t.Case).ThenInclude(c => c!.AssignedOfficer)
                .Include(t => t.AssignedUser)
                .Include(t => t.AssignedRole)
                .Include(t => t.WorkflowInstanceLevel)
                    .ThenInclude(il => il.WorkflowLevel)
                        .ThenInclude(wl => wl.Targets)
                .AsNoTracking()
                .Where(t => t.TaskStatus == WorkflowLevelStatus.Pending && t.Case != null && t.Case.Status != CaseStatus.Closed);

            if (!isSuperAdmin)
            {
                recordedTasksQuery = recordedTasksQuery.Where(t =>
                    t.AssignedUserId == currentUserId
                    || (userRoleId.HasValue && t.AssignedRoleId == userRoleId.Value)
                    || (t.Case!.AssignedOfficer != null && t.Case.AssignedOfficer.UserId == currentUserId)
                    || (t.WorkflowInstanceLevel != null && eligibleLevelNumbers.Contains(t.WorkflowInstanceLevel.WorkflowLevel.LevelNumber)));
            }

            var recordedTasks = await recordedTasksQuery
                .OrderByDescending(t => t.CreatedAt)
                .Take(isSuperAdmin ? 100 : 50)
                .ToListAsync(cancellationToken);

            var taskList = new List<CaseApprovalTaskDto>();

            // Map existing recorded tasks
            foreach (var t in recordedTasks)
            {
                var level = t.WorkflowInstanceLevel?.WorkflowLevel;
                var roleName = t.AssignedRole?.Name
                            ?? (level != null && !string.IsNullOrWhiteSpace(level.Name) ? level.Name : (!string.IsNullOrWhiteSpace(t.Case?.CurrentStage) ? t.Case.CurrentStage : "Officer Queue"));

                var userName = t.AssignedUser != null
                    ? $"{t.AssignedUser.FirstName} {t.AssignedUser.LastName}".Trim()
                    : roleName;

                taskList.Add(new CaseApprovalTaskDto(
                    t.Id,
                    t.WorkflowInstanceId,
                    t.WorkflowInstanceLevelId,
                    t.CaseId,
                    t.Case?.Subject ?? "Case",
                    t.AssignedUserId ?? Guid.Empty,
                    userName,
                    t.AssignedRoleId,
                    roleName,
                    t.Action,
                    t.TaskStatus,
                    t.Comment,
                    t.PerformedAt,
                    t.CreatedAt
                ));
            }

            // 5. Virtual synthesis for open cases without a recorded task (strictly bounded)
            var existingTaskCaseIds = recordedTasks.Select(t => t.CaseId).ToHashSet();
            var openCasesQuery = _context.Cases
                .Include(c => c.AssignedOfficer)
                .AsNoTracking()
                .Where(c => !existingTaskCaseIds.Contains(c.Id) && c.Status != CaseStatus.Closed);

            if (!isSuperAdmin)
            {
                // Only load open cases assigned to this officer or matching the officer's eligible workflow stages
                var eligibleStageSlugs = eligibleLevelNumbers
                    .Select(GetStageSlugForLevel)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToHashSet();

                openCasesQuery = openCasesQuery.Where(c =>
                    (c.AssignedOfficer != null && c.AssignedOfficer.UserId == currentUserId)
                    || eligibleStageSlugs.Contains(c.CurrentStage));
            }

            var openCasesWithoutTask = await openCasesQuery
                .OrderByDescending(c => c.CreatedAt)
                .Take(isSuperAdmin ? 50 : 25)
                .ToListAsync(cancellationToken);

            foreach (var @case in openCasesWithoutTask)
            {
                int stageLevelNum = GetStageLevelNumber(@case.CurrentStage);

                // For non-super admins, ensure the case stage actually matches an eligible level or direct assignment
                if (!isSuperAdmin)
                {
                    bool isCaseOfficer = @case.AssignedOfficer?.UserId == currentUserId;
                    bool isEligibleLevel = eligibleLevelNumbers.Contains(stageLevelNum);
                    if (!isCaseOfficer && !isEligibleLevel)
                    {
                        continue;
                    }
                }

                var matchingLevel = activeWorkflow?.Levels.FirstOrDefault(l => l.LevelNumber == stageLevelNum)
                                 ?? activeWorkflow?.Levels.FirstOrDefault();

                var roleTarget = matchingLevel?.Targets.FirstOrDefault(tgt => tgt.TargetType == WorkflowLevelTargetType.Role);
                string stageRoleName = matchingLevel != null && !string.IsNullOrWhiteSpace(matchingLevel.Name)
                    ? matchingLevel.Name
                    : (!string.IsNullOrWhiteSpace(@case.CurrentStage) ? @case.CurrentStage : "Assigned Officer");

                taskList.Add(new CaseApprovalTaskDto(
                    Guid.NewGuid(),
                    @case.ActiveWorkflowInstanceId ?? Guid.Empty,
                    Guid.Empty,
                    @case.Id,
                    @case.Subject ?? "Case",
                    @case.AssignedOfficer?.UserId ?? Guid.Empty,
                    @case.AssignedOfficer != null ? "Assigned Officer" : stageRoleName,
                    roleTarget?.TargetId,
                    stageRoleName,
                    WorkflowAction.Approve,
                    WorkflowLevelStatus.Pending,
                    "Pending stage review",
                    null,
                    @case.CreatedAt
                ));
            }

            // Deduplicate by CaseId so each case appears at most once in the queue
            var distinctTasks = taskList
                .GroupBy(t => t.CaseId)
                .Select(g => g.OrderByDescending(t => t.CreatedAt).First())
                .ToList();

            if (isSuperAdmin)
            {
                return distinctTasks;
            }

            // Final eligibility verification against active workflow targets
            var recordedTaskLookup = recordedTasks.GroupBy(rt => rt.CaseId).ToDictionary(g => g.Key, g => g.First());

            var filteredTasks = distinctTasks.Where(dto =>
            {
                // Directly assigned to this user
                if (dto.AssignedUserId != Guid.Empty && dto.AssignedUserId == currentUserId) return true;

                // Directly assigned to this user's role
                if (userRoleId.HasValue && dto.AssignedRoleId.HasValue && dto.AssignedRoleId.Value == userRoleId.Value) return true;

                // Case assigned officer
                if (recordedTaskLookup.TryGetValue(dto.CaseId, out var origTask))
                {
                    var caseOfficerUserId = origTask.Case?.AssignedOfficer?.UserId;
                    if (caseOfficerUserId.HasValue && caseOfficerUserId.Value == currentUserId) return true;

                    var level = origTask.WorkflowInstanceLevel?.WorkflowLevel;
                    if (level != null && user != null && IsUserEligibleForLevel(user, level))
                    {
                        return origTask.Case?.AssignedOfficerId == null
                            || (origTask.Case?.AssignedOfficer != null && origTask.Case.AssignedOfficer.UserId == currentUserId)
                            || origTask.AssignedUserId == null
                            || origTask.AssignedUserId == currentUserId;
                    }
                }
                else
                {
                    // Synthetic task check against active workflow level
                    var stageLevel = GetStageLevelNumber(dto.AssignedRoleName);
                    if (eligibleLevelNumbers.Contains(stageLevel)) return true;
                }

                return false;
            }).ToList();

            return filteredTasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pending approval tasks");
            return new List<CaseApprovalTaskDto>();
        }
    }

    private static bool IsUserEligibleForLevel(User user, WorkflowLevel level)
    {
        var targets = level.Targets?.ToList() ?? new List<WorkflowLevelTarget>();
        if (!targets.Any())
        {
            return false;
        }

        var userTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.User).Select(t => t.TargetId).ToHashSet();
        var roleTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Role).Select(t => t.TargetId).ToHashSet();
        var deptTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Department).Select(t => t.TargetId).ToHashSet();

        if (userTargetIds.Contains(user.Id)) return true;

        bool roleMatch = user.RoleId.HasValue && roleTargetIds.Contains(user.RoleId.Value);
        bool deptMatch = !deptTargetIds.Any() || (user.DepartmentId.HasValue && deptTargetIds.Contains(user.DepartmentId.Value));

        if (roleTargetIds.Any() && deptTargetIds.Any())
        {
            return roleMatch && deptMatch;
        }
        if (roleTargetIds.Any())
        {
            return roleMatch;
        }
        if (deptTargetIds.Any())
        {
            return deptMatch;
        }

        return false;
    }

    private static int GetStageLevelNumber(string? stage)
    {
        if (string.IsNullOrWhiteSpace(stage)) return 1;
        var s = stage.Trim().ToLowerInvariant();
        if (s.StartsWith("1") || s.Contains("intake")) return 1;
        if (s.StartsWith("2") || s.Contains("regist")) return 2;
        if (s.StartsWith("3") || s.Contains("initial") || s.Contains("review")) return 3;
        if (s.StartsWith("4") || s.Contains("admissib") || s.Contains("jurisdiction")) return 4;
        if (s.StartsWith("5") || s.Contains("investig") || s.Contains("resolution")) return 5;
        if (s.StartsWith("6") || s.Contains("decision") || s.Contains("communication")) return 6;
        if (s.StartsWith("7") || s.Contains("clos") || s.Contains("archiv")) return 7;
        return 1;
    }

    private static string GetStageSlugForLevel(int level) => level switch
    {
        1 => WorkflowStage.Intake,
        2 => WorkflowStage.RegistrationAndAcknowledgement,
        3 => WorkflowStage.InitialReviewAndAssignment,
        4 => WorkflowStage.JurisdictionAndAdmissibility,
        5 => WorkflowStage.InvestigationAndResolution,
        6 => WorkflowStage.DecisionAndCommunication,
        7 => WorkflowStage.ClosureAndArchiving,
        _ => WorkflowStage.Intake
    };
}
