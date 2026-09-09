using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class CasesService : ICasesService
{
    private readonly IGenericRepository<Case> _caseRepo;
    private readonly IGenericRepository<Complaint> _complaintRepo;
    private readonly IGenericRepository<CaseFinding> _findingRepo;
    private readonly IGenericRepository<CaseMilestone> _milestoneRepo;
    private readonly IGenericRepository<CaseCommunicationLog> _communicationRepo;
    private readonly IGenericRepository<Document> _docRepo;
    private readonly IGenericRepository<CaseNote> _noteRepo;
    private readonly IGenericRepository<CaseStatusHistory> _historyRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<TaxpayerProfile> _taxpayerProfileRepo;
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _storage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CasesService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _context;

    public CasesService(
        IGenericRepository<Case> caseRepo,
        IGenericRepository<Complaint> complaintRepo,
        IGenericRepository<CaseFinding> findingRepo,
        IGenericRepository<CaseMilestone> milestoneRepo,
        IGenericRepository<CaseCommunicationLog> communicationRepo,
        IGenericRepository<Document> docRepo,
        IGenericRepository<CaseNote> noteRepo,
        IGenericRepository<CaseStatusHistory> historyRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<TaxpayerProfile> taxpayerProfileRepo,
        IGenericRepository<Account> accountRepo,
        UserManager<User> userManager,
        IEmailService emailService,
        IFileStorageService storage,
        IConfiguration configuration,
        ILogger<CasesService> logger,
        ICurrentUser currentUser,
        IApplicationDbContext context)
    {
        _caseRepo = caseRepo;
        _complaintRepo = complaintRepo;
        _findingRepo = findingRepo;
        _milestoneRepo = milestoneRepo;
        _communicationRepo = communicationRepo;
        _docRepo = docRepo;
        _noteRepo = noteRepo;
        _historyRepo = historyRepo;
        _userRepo = userRepo;
        _taxpayerProfileRepo = taxpayerProfileRepo;
        _accountRepo = accountRepo;
        _userManager = userManager;
        _emailService = emailService;
        _storage = storage;
        _configuration = configuration;
        _logger = logger;
        _currentUser = currentUser;
        _context = context;
    }




    // ─── Queries ───────────────────────────────────────────────────────────────

    private static bool IsUserEligibleForLevel(User user, WorkflowLevel level)
    {
        var targets = level.Targets.ToList();
        if (!targets.Any())
        {
            // No specific targets defined on this stage level -> open to all staff users
            return true;
        }

        var userTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.User).Select(t => t.TargetId).ToHashSet();
        var roleTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Role).Select(t => t.TargetId).ToHashSet();
        var deptTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Department).Select(t => t.TargetId).ToHashSet();

        // 1. Direct user assignment target
        if (userTargetIds.Contains(user.Id))
            return true;

        // 2. Department + Role intersection
        if (roleTargetIds.Any() && deptTargetIds.Any())
        {
            return user.RoleId.HasValue && roleTargetIds.Contains(user.RoleId.Value)
                && user.DepartmentId.HasValue && deptTargetIds.Contains(user.DepartmentId.Value);
        }

        // 3. Role target only
        if (roleTargetIds.Any())
        {
            return user.RoleId.HasValue && roleTargetIds.Contains(user.RoleId.Value);
        }

        // 4. Department target only
        if (deptTargetIds.Any())
        {
            return user.DepartmentId.HasValue && deptTargetIds.Contains(user.DepartmentId.Value);
        }

        return false;
    }

    private static bool IsUserEligibleForStageDefault(string roleName, string stage)
    {
        var r = (roleName ?? string.Empty).ToLowerInvariant();
        return stage switch
        {
            WorkflowStage.Intake => r.Contains("officer") || r.Contains("intake") || r.Contains("registry") || r.Contains("front"),
            WorkflowStage.RegistrationAndAcknowledgement => r.Contains("officer") || r.Contains("registrar") || r.Contains("registry"),
            WorkflowStage.InitialReviewAndAssignment => r.Contains("manager") || r.Contains("director") || r.Contains("chief") || r.Contains("ce"),
            WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => r.Contains("legal") || r.Contains("senior") || r.Contains("manager"),
            WorkflowStage.InvestigationAndResolution => r.Contains("senior") || r.Contains("investigat") || r.Contains("operation"),
            WorkflowStage.DecisionAndCommunication => r.Contains("manager") || r.Contains("director") || r.Contains("chief") || r.Contains("ce"),
            WorkflowStage.ClosureAndArchiving => r.Contains("director") || r.Contains("chief") || r.Contains("ce") || r.Contains("registrar") || r.Contains("registry"),
            _ => true
        };
    }

    private async Task<IQueryable<Case>> ApplyStageQueueFilterAsync(IQueryable<Case> query, string stage, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser?.UserId;
        if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty)
            return query;

        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, cancellationToken);

        if (user == null)
            return query;

        var roleName = user.Role?.Name ?? string.Empty;
        var isSuperAdmin = (_currentUser != null && (_currentUser.IsInRole("Super Admin") || _currentUser.IsInRole("Admin")))
                        || roleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)
                        || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        if (isSuperAdmin)
            return query;

        LevelRole? levelRole = stage switch
        {
            WorkflowStage.Intake => LevelRole.Intake,
            WorkflowStage.RegistrationAndAcknowledgement => LevelRole.Registration,
            WorkflowStage.InitialReviewAndAssignment => LevelRole.InitialReview,
            WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => LevelRole.AdmissibilityGate,
            WorkflowStage.InvestigationAndResolution => LevelRole.Investigation,
            WorkflowStage.DecisionAndCommunication => LevelRole.Decision,
            WorkflowStage.ClosureAndArchiving => LevelRole.Closure,
            _ => null
        };

        int? levelNumber = stage switch
        {
            WorkflowStage.Intake => 1,
            WorkflowStage.RegistrationAndAcknowledgement => 2,
            WorkflowStage.InitialReviewAndAssignment => 3,
            WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => 4,
            WorkflowStage.InvestigationAndResolution => 5,
            WorkflowStage.DecisionAndCommunication => 6,
            WorkflowStage.ClosureAndArchiving => 7,
            _ => null
        };

        var activeLevels = await _context.WorkflowLevels
            .Include(l => l.Targets)
            .Include(l => l.Workflow)
            .Where(l => l.Workflow.IsActive && !l.Workflow.IsDeleted &&
                       ((levelRole.HasValue && l.LevelRole == levelRole.Value) ||
                        (levelNumber.HasValue && l.LevelNumber == levelNumber.Value)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        bool isEligibleForStage = activeLevels.Any()
            ? activeLevels.Any(l => IsUserEligibleForLevel(user, l))
            : IsUserEligibleForStageDefault(roleName, stage);

        var uid = currentUserId.Value;
        var rid = user.RoleId;

        if (isEligibleForStage)
        {
            // Officer's role is assigned to this stage:
            // Sees cases assigned to them, cases with pending tasks for them or their role, or unassigned pool cases in this stage
            return query.Where(c =>
                (c.AssignedOfficer != null && c.AssignedOfficer.UserId == uid) ||
                _context.CaseApprovalTasks.Any(t => t.CaseId == c.Id && t.TaskStatus == WorkflowLevelStatus.Pending &&
                    ((t.AssignedUserId.HasValue && t.AssignedUserId.Value == uid) ||
                     (rid.HasValue && t.AssignedRoleId.HasValue && t.AssignedRoleId.Value == rid.Value))) ||
                c.AssignedOfficerId == null
            );
        }
        else
        {
            // Officer's role is NOT assigned to this stage:
            // Can ONLY see cases if specifically assigned to them as officer or task assignee
            return query.Where(c =>
                (c.AssignedOfficer != null && c.AssignedOfficer.UserId == uid) ||
                _context.CaseApprovalTasks.Any(t => t.CaseId == c.Id && t.TaskStatus == WorkflowLevelStatus.Pending &&
                    t.AssignedUserId.HasValue && t.AssignedUserId.Value == uid)
            );
        }
    }

    public async Task<Response<PagedResult<CaseListDto>>> GetCasesAsync(GetCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.AssignedOfficer)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c =>
                    (c.CaseNumber != null && c.CaseNumber.Value.Contains(request.Search)) ||
                    c.Complaint.ReferenceNumber.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Stage))
            {
                query = query.Where(c => c.CurrentStage == request.Stage);
                query = await ApplyStageQueueFilterAsync(query, request.Stage, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            var total = await query.CountAsync(cancellationToken);

            var items = await (
                from c in query.OrderByDescending(c => c.CreatedAt)
                                .Skip((request.Page - 1) * request.PageSize)
                                .Take(request.PageSize)
                join co in _complaintRepo.Query()
                    on c.ComplaintId equals co.Id
                join tp in _taxpayerProfileRepo.Query()
                    on co.TaxpayerId equals tp.Id into tpGroup
                from tp in tpGroup.DefaultIfEmpty()
                join u in _userRepo.Query()
                    on (tp != null ? tp.UserId : (Guid?)null) equals u.Id into uGroup
                from u in uGroup.DefaultIfEmpty()
                select new CaseListDto(
                    c.Id,
                    c.CaseNumber != null ? c.CaseNumber.Value : co.ReferenceNumber,
                    c.ComplaintId,
                    co.ReferenceNumber,
                    u != null ? $"{u.FirstName} {u.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null && c.AssignedOfficer.User != null
                        ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName
                        : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                )
            ).ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CasesRetrieved;
            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cases in GetCasesAsync");
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<PagedResult<CaseListDto>>> GetMyCasesAsync(GetMyCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.AssignedOfficer)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            var currentUserId = _currentUser?.UserId;
            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, cancellationToken);

                var roleName = user?.Role?.Name ?? string.Empty;
                var isSuperAdmin = (_currentUser != null && (_currentUser.IsInRole("Super Admin") || _currentUser.IsInRole("Admin")))
                                || roleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)
                                || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                if (!isSuperAdmin)
                {
                    var uid = currentUserId.Value;
                    query = query.Where(c =>
                        (c.AssignedOfficer != null && c.AssignedOfficer.UserId == uid) ||
                        _context.CaseApprovalTasks.Any(t => t.CaseId == c.Id && t.TaskStatus == WorkflowLevelStatus.Pending &&
                            t.AssignedUserId.HasValue && t.AssignedUserId.Value == uid)
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c =>
                    (c.CaseNumber != null && c.CaseNumber.Value.Contains(request.Search)) ||
                    c.Complaint.ReferenceNumber.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Stage))
                query = query.Where(c => c.CurrentStage == request.Stage);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            var total = await query.CountAsync(cancellationToken);

            var items = await (
                from c in query.OrderByDescending(c => c.CreatedAt)
                                .Skip((request.Page - 1) * request.PageSize)
                                .Take(request.PageSize)
                join co in _complaintRepo.Query()
                    on c.ComplaintId equals co.Id
                join tp in _taxpayerProfileRepo.Query()
                    on co.TaxpayerId equals tp.Id into tpGroup
                from tp in tpGroup.DefaultIfEmpty()
                join u in _userRepo.Query()
                    on (tp != null ? tp.UserId : (Guid?)null) equals u.Id into uGroup
                from u in uGroup.DefaultIfEmpty()
                select new CaseListDto(
                    c.Id,
                    c.CaseNumber != null ? c.CaseNumber.Value : co.ReferenceNumber,
                    c.ComplaintId,
                    co.ReferenceNumber,
                    u != null ? $"{u.FirstName} {u.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null && c.AssignedOfficer.User != null
                        ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName
                        : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                )
            ).ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CasesRetrieved;
            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<PagedResult<CaseListDto>>> GetOverdueCasesAsync(GetOverdueCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer).ThenInclude(tp => tp.User)
                .Where(c => c.DueDate.HasValue && c.DueDate < now);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(c => c.DueDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CaseListDto(
                    c.Id,
                    c.CaseNumber.Value,
                    c.ComplaintId,
                    c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer != null && c.Complaint.Taxpayer.User != null ? $"{c.Complaint.Taxpayer.User.FirstName} {c.Complaint.Taxpayer.User.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
            response.StatusCode = StatusCodes.Status200OK;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<QueueResultDto>> GetQueueAsync(GetQueueQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<QueueResultDto>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer).ThenInclude(tp => tp.User)
                .Include(c => c.AssignedOfficer)
                .Where(c => c.CurrentStage == request.QueueName);

            query = await ApplyStageQueueFilterAsync(query, request.QueueName, cancellationToken);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new QueueItemDto(
                    c.Id,
                    c.Complaint.ReferenceNumber,
                    c.Subject,
                    "",
                    "",
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.Complaint.Taxpayer != null && c.Complaint.Taxpayer.User != null ? $"{c.Complaint.Taxpayer.User.FirstName} {c.Complaint.Taxpayer.User.LastName}" : "Unknown",
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseQueueRetrieved;
            response.Data = new QueueResultDto(request.QueueName, items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<CaseDetailDto>> GetCaseByIdAsync(GetCaseByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CaseDetailDto>();
        try
        {
            var c = await _caseRepo.Query()
                .Include(x => x.Complaint).ThenInclude(co => co.Taxpayer).ThenInclude(tp => tp.User)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .Include(x => x.Department)
                .Include(x => x.Findings)
                .Include(x => x.Recommendations)
                .Include(x => x.Milestones)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.Id == request.Id || x.ComplaintId == request.Id, cancellationToken);


            if (c is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseRetrieved;
            response.Data = new CaseDetailDto(
                c.Id,
                c.CaseNumber.Value,
                c.Subject,
                c.Summary,
                c.Priority,
                c.Status.ToString(),
                c.CurrentStage,
                c.AssignedOfficer is null ? null : new CaseOfficerDto(c.AssignedOfficer.Id, $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}", c.AssignedOfficer.User.Email ?? string.Empty),
                c.Department is null ? null : new CaseDepartmentDto(c.Department.Id, c.Department.Name),
                c.DueDate,
                c.ClosedAt,
                c.Outcome,
                c.FindingsSummary,
                new CaseComplaintDto(c.Complaint.Id, c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer is null ? null : new ComplaintTaxpayerDto(c.Complaint.Taxpayer.Id, c.Complaint.Taxpayer.User != null ? $"{c.Complaint.Taxpayer.User.FirstName} {c.Complaint.Taxpayer.User.LastName}" : "Unknown", c.Complaint.Taxpayer.User?.Email ?? ""),
                    c.Complaint.TaxType, c.Complaint.TaxPeriod, c.Complaint.ComplaintCategory),
                c.Findings.Select(f => new FindingDto(f.Id, f.Description, f.CreatedAt)),
                c.Recommendations.Select(r => new RecommendationDto(r.Id, r.RecommendationText, r.ApprovedByUserId ?? Guid.Empty, r.CreatedAt)),
                c.Milestones.Select(m => new MilestoneDto(m.Id, m.Title, m.Description, m.CreatedAt)),
                c.StatusHistory.Select(h => new StatusHistoryDto(h.Id, h.OldStatus.ToString(), h.NewStatus.ToString(), h.ChangedByUserId, h.TransitionedAt))
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseFindingDto>>> GetCaseFindingsAsync(GetCaseFindingsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseFindingDto>>();
        try
        {
            var findings = await _findingRepo.Query()
                .Where(f => f.CaseId == request.CaseId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new CaseFindingDto(f.Id, f.CaseId, f.Description, f.CreatedAt, f.CreatedByUserId))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingsRetrieved;
            response.Data = findings.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingsError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseMilestoneDto>>> GetCaseMilestonesAsync(GetCaseMilestonesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseMilestoneDto>>();
        try
        {
            var milestones = await _milestoneRepo.Query()
                .Where(m => m.CaseId == request.CaseId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new CaseMilestoneDto(m.Id, m.CaseId, m.Title, m.Description, m.TargetDate, m.CompletedAt, m.IsCompleted))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseMilestonesRetrieved;
            response.Data = milestones.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseCommunicationDto>>> GetCaseCommunicationsAsync(GetCaseCommunicationsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseCommunicationDto>>();
        try
        {
            var logs = await _communicationRepo.Query()
                .Where(l => l.CaseId == request.CaseId)
                .OrderByDescending(l => l.SentAt)
                .Select(l => new CaseCommunicationDto(l.Id, l.CaseId, l.Sender, l.Recipient, l.Direction.ToString(), l.Subject, l.Body, l.SentAt, l.Channel))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseCommsRetrieved;
            response.Data = logs.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseDocumentDto>>> GetCaseDocumentsAsync(GetCaseDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseDocumentDto>>();
        try
        {
            var entityIds = new HashSet<Guid> { request.CaseId };
            var caseItem = await _caseRepo.Query()
                .Where(c => c.Id == request.CaseId || c.ComplaintId == request.CaseId)
                .Select(c => new { c.Id, c.ComplaintId })
                .FirstOrDefaultAsync(cancellationToken);
            if (caseItem != null)
            {
                entityIds.Add(caseItem.Id);
                entityIds.Add(caseItem.ComplaintId);
            }

            var documents = await _docRepo.Query()
                .Where(d => entityIds.Contains(d.EntityId))
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new CaseDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentsRetrieved;
            response.Data = documents.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocsError;
        }
        return response;
    }

    public async Task<Response<TrackComplaintResponse>> TrackComplaintAsync(TrackComplaintQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TrackComplaintResponse>();
        try
        {
            var trackingNo = request.TrackingNumber?.Trim();
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Please enter a valid Complaint ID or Reference Number.";
                return response;
            }

            var isGuid = Guid.TryParse(trackingNo, out var idGuid);

            // Query Complaint repository cleanly (no unneeded includes or untranslatable functions)
            var complaint = await _complaintRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => 
                    c.ReferenceNumber == trackingNo ||
                    (isGuid && c.Id == idGuid), cancellationToken);

            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = $"No complaint found matching ID '{trackingNo}'. Please verify your Complaint ID and try again.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint found!";
            response.Data = new TrackComplaintResponse(
                complaint.ReferenceNumber,
                complaint.Status.ToString(),
                complaint.CurrentStage,
                complaint.Description,
                complaint.CreatedAt,
                complaint.LastModifiedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }


    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<SubmitPublicCaseResponse>> SubmitPublicCaseAsync(SubmitPublicCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SubmitPublicCaseResponse>();
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Description))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Email and complaint description are required.";
                return response;
            }

            // 1. Ensure User & TaxpayerProfile exist for public submitter
            var emailNormalized = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(emailNormalized);
            if (user == null)
            {
                var defaultPass = $"TaxOmbudPass@{Guid.NewGuid().ToString()[..8]}!";
                var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? "Public" : request.FirstName;
                var lastName = string.IsNullOrWhiteSpace(request.LastName) ? "Taxpayer" : request.LastName;

                user = User.Create(firstName, lastName, new Email(request.Email), phone: request.Phone, userType: UserType.RegisteredTaxpayer);

                var createResult = await _userManager.CreateAsync(user, defaultPass);
                if (!createResult.Succeeded)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return response;
                }
            }

            var taxpayerProfile = await _taxpayerProfileRepo.Query().FirstOrDefaultAsync(tp => tp.UserId == user.Id, cancellationToken);
            if (taxpayerProfile == null)
            {
                taxpayerProfile = TaxpayerProfile.Create(user.Id, request.SubmitterType ?? "Personal");
                taxpayerProfile.Nin = request.Nin;
                taxpayerProfile.TinNumber = request.TaxId;
                taxpayerProfile.CompanyName = request.OrgName;
                taxpayerProfile.RcNumber = request.CacNumber;
                taxpayerProfile.Country = request.CountryId;
                taxpayerProfile.State = request.StateId;

                await _taxpayerProfileRepo.AddAsync(taxpayerProfile);
                await _taxpayerProfileRepo.SaveAsync();
            }

            var refNumber = $"TOC-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var subjectText = !string.IsNullOrWhiteSpace(request.Subject)
                ? request.Subject
                : request.Description[..Math.Min(80, request.Description.Length)];

            var complaint = Complaint.Create(
                taxpayerId: taxpayerProfile.Id,
                taxType: request.ComplaintType ?? "Tax Dispute",
                taxPeriod: $"{DateTimeOffset.UtcNow.Year}",
                category: request.ServiceDomain ?? "General",
                subject: subjectText,
                description: request.Description,
                referenceNumber: refNumber,
                intakeChannel: IntakeChannel.OnlinePortal,
                taxOfficeRef: null,
                tinNumber: request.TaxId,
                whyOtoHandle: request.OtoReason
            );

            complaint.Submit();
            complaint.UpdateStage("1_intake");

            await _complaintRepo.AddAsync(complaint);
            await _complaintRepo.SaveAsync();

            // Resolve active default Account for Case workflow lane
            var account = await _accountRepo.Query().FirstOrDefaultAsync(cancellationToken);
            if (account == null)
            {
                account = new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Headquarters Zonal Office",
                    Email = "info@mediate.com.ng",
                    Country = "Nigeria",
                    Status = "active",
                    IsWorkflowLane = true
                };
                await _accountRepo.AddAsync(account);
                await _accountRepo.SaveAsync();
            }

            // Also create underlying Case entity linked to Complaint and Account
            var caseNumberStr = $"CASE-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var caseEntity = new Case(
                complaint.Id,
                subjectText,
                account.Id,
                request.Priority ?? "Medium"
            );
            caseEntity.Open(ReferenceNumber.From(caseNumberStr));
            caseEntity.UpdateStatus(CaseStatus.Submitted, "1_intake", Guid.Empty);

            await _caseRepo.AddAsync(caseEntity);
            await _caseRepo.SaveAsync();

            // Upload any attached files, linked to the complaint entity
            if (request.Attachments is { Count: > 0 })
            {
                await UploadCaseDocumentsAsync(
                    new UploadCaseDocumentsCommand(caseEntity.Id, request.Attachments), cancellationToken);
            }

            // Dispatch Lodgement Receipt Email to Taxpayer
            var baseUrl = Helper.GetAppBaseUrl(_configuration);
            var fullName = $"{request.FirstName} {request.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = "Valued Taxpayer";

            var htmlBody = $"""
                <div style="font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;">
                  <div style="background:#114a31;padding:24px 32px;text-align:center;border-bottom:4px solid #c9a227;">
                    <h1 style="color:#ffffff;font-size:1.2rem;margin:0 0 4px;letter-spacing:.5px;text-transform:uppercase;">OFFICE OF THE TAX OMBUD</h1>
                    <p style="color:rgba(255,255,255,.75);font-size:.85rem;margin:0;">Federal Republic of Nigeria</p>
                  </div>
                  <div style="padding:32px;background:#ffffff;color:#333333;font-size:.95rem;line-height:1.7;">
                    <h2 style="color:#114a31;font-size:1.2rem;margin-top:0;">Complaint Lodgement Acknowledgement</h2>
                    <p>Dear <strong>{fullName}</strong>,</p>
                    <p>Your tax complaint has been successfully submitted to the Tax Ombud Office portal.</p>
                    <div style="background:#f8f9fa;border-left:4px solid #114a31;padding:16px 20px;margin:24px 0;border-radius:4px;">
                      <p style="margin:0 0 8px;"><strong>Complaint Tracking Number:</strong> <code style="background:#e9ecef;padding:3px 8px;border-radius:4px;font-weight:bold;color:#114a31;font-size:1.05rem;">{refNumber}</code></p>
                      <p style="margin:0 0 8px;"><strong>Subject:</strong> {Regex.Replace(subjectText ?? "", "<.*?>|&nbsp;", " ").Trim()}</p>
                      <p style="margin:0;"><strong>Date Lodged:</strong> {DateTimeOffset.UtcNow:dd MMMM yyyy, HH:mm UTC}</p>
                    </div>
                    <p>You can track the live status of your complaint at any time by clicking the button below or using your tracking number on our portal:</p>
                    <div style="text-align:center;margin:28px 0;">
                      <a href="{baseUrl}/track-complaint?ref={refNumber}" target="_blank" style="background:#114a31;color:#ffffff;text-decoration:none;padding:12px 28px;border-radius:6px;font-weight:bold;display:inline-block;font-size:.95rem;box-shadow:0 2px 4px rgba(0,0,0,0.15);">
                        Track Complaint Status →
                      </a>
                    </div>
                  </div>

                  <div style="background:#114a31;padding:20px 32px;text-align:center;">
                    <p style="color:#c9a227;font-size:.9rem;font-weight:bold;margin:4px 0;">Office of the Tax Ombud</p>
                    <p style="color:rgba(255,255,255,.6);font-size:.75rem;margin:4px 0;">Federal Republic of Nigeria</p>
                  </div>
                </div>
                """;

            try
            {
                await _emailService.SendAsync(
                    to: request.Email,
                    subject: $"Complaint Lodgement Acknowledgement — Ref: {refNumber}",
                    htmlBody: htmlBody,
                    cancellationToken: cancellationToken);

                // Send audit copy to Registry Intake Desk
                var adminNotice = $"""
                    <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                      <h3 style="color:#114a31;margin-top:0;">New Public Complaint Lodged</h3>
                      <p><strong>Tracking Ref:</strong> {refNumber}</p>
                      <p><strong>Complainant:</strong> {fullName} ({request.Email}, {request.Phone})</p>
                      <p><strong>Mode:</strong> {request.SubmitterType}</p>
                      <p><strong>Subject:</strong> {subjectText}</p>
                    </div>
                    """;
                await _emailService.SendAsync("registry@mediate.com.ng", $"[New Complaint Intake] Ref: {refNumber}", adminNotice, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send lodgement email for complaint ref {RefNumber}", refNumber);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint submitted successfully and lodgement receipt email dispatched.";
            response.Data = new SubmitPublicCaseResponse(complaint.Id, refNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process public complaint submission");
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseSubmitError;
        }
        return response;
    }


    public async Task<Response<AddCaseNoteResponse>> AddCaseNoteAsync(AddCaseNoteCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AddCaseNoteResponse>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            var note = new CaseNote
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                Content = request.Text,
                IsInternal = !request.IsExternal,
                CreatedAt = DateTime.UtcNow
            };

            await _noteRepo.AddAsync(note);
            await _noteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.NoteAdded;
            response.Data = new AddCaseNoteResponse(note.Id, note.Content, !note.IsInternal, note.CreatedAt);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseNoteAddError;
        }
        return response;
    }

    public async Task<Response<Guid>> AddCaseFindingAsync(AddCaseFindingCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var finding = new CaseFinding
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _findingRepo.AddAsync(finding);
            await _findingRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingAdded;
            response.Data = finding.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingAddError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateCaseFindingAsync(UpdateCaseFindingCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var finding = await _findingRepo.Query()
                .FirstOrDefaultAsync(f => f.Id == request.FindingId && f.CaseId == request.CaseId, cancellationToken);

            if (finding is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseFindingNotFound;
                return response;
            }

            finding.Description = request.Description;
            await _findingRepo.UpdateAsync(finding);
            await _findingRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingUpdated;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingUpdateError;
        }
        return response;
    }

    public async Task<Response<object?>> AssignCaseAsync(AssignCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            caseEntity.Assign(request.OfficerId, Guid.Empty);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseAssigned;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseAssignError;
        }
        return response;
    }

    public async Task<Response<object?>> TransitionCaseAsync(TransitionCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            var previousStatus = caseEntity.Status;
            caseEntity.UpdateStatus(caseEntity.Status, request.TargetStage, Guid.Empty);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            var complaint = await _complaintRepo.GetByIdAsync(caseEntity.ComplaintId);
            if (complaint != null)
            {
                complaint.UpdateStatus(caseEntity.Status, request.TargetStage);
                await _complaintRepo.UpdateAsync(complaint);
                await _complaintRepo.SaveAsync();
            }

            // Write an immutable audit log entry for the transition.
            var historyEntry = new CaseStatusHistory
            {
                Id = Guid.NewGuid(),
                CaseId = caseEntity.Id,
                OldStatus = previousStatus,
                NewStatus = caseEntity.Status,
                ChangedByUserId = Guid.Empty,
                TransitionedAt = DateTimeOffset.UtcNow,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };
            await _historyRepo.AddAsync(historyEntry);
            await _historyRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = string.Format(Constants.Messages.CaseUpdated);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseTransitionError;
        }
        return response;
    }

    public async Task<Response<PostRecommendationResponse>> PostRecommendationAsync(PostRecommendationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PostRecommendationResponse>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            var rec = new CaseRecommendation
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                RecommendationText = request.RecommendationText,
                CreatedAt = DateTime.UtcNow
            };

            caseEntity.Recommendations.Add(rec);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseRecommendationPosted;
            response.Data = new PostRecommendationResponse(rec.Id, rec.RecommendationText, rec.CreatedAt);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseRecommendationError;
        }
        return response;
    }

    public async Task<Response<object?>> ApproveClosureAsync(ApproveClosureCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            if (request.Rationale.Length < 100)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.CaseCeApprovalRationaleShort;
                return response;
            }

            if (request.Approve)
            {
                caseEntity.Close("Approved for closure", request.Rationale, Guid.Empty);
            }

            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = request.Approve ? "Case closure approved." : "Case closure rejected.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseClosureApprovalError;
        }
        return response;
    }

    public async Task<Response<object?>> CompleteMilestoneAsync(CompleteMilestoneCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var milestone = await _milestoneRepo.Query()
                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId && m.CaseId == request.CaseId, cancellationToken);

            if (milestone is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseMilestoneNotFound;
                return response;
            }

            milestone.IsCompleted = true;
            milestone.CompletedAt = DateTimeOffset.UtcNow;
            await _milestoneRepo.UpdateAsync(milestone);
            await _milestoneRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseMilestoneCompleted;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseMilestoneCompleteError;
        }
        return response;
    }

    public async Task<Response<Guid>> UploadCaseDocumentAsync(UploadCaseDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            await using var stream = request.File.OpenReadStream();
            var path = await _storage.StoreAsync(stream, request.File.FileName, request.File.ContentType, cancellationToken);

            var docId = Guid.NewGuid();
            var doc = new Document
            {
                Id = docId,
                EntityId = request.CaseId,
                EntityType = DocumentEntityType.Case,
                FileName = request.File.FileName,
                FilePath = path,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                CreatedAt = DateTime.UtcNow
            };

            await _docRepo.AddAsync(doc);
            await _docRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentUploaded;
            response.Data = docId;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocUploadError;
        }
        return response;
    }

    public async Task<Response<PagedResult<CaseHistoryListDto>>> GetCaseHistoryAsync(GetCaseHistoryQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseHistoryListDto>>();
        try
        {
            var query = _historyRepo.Query()
                .Include(h => h.Case)
                    .ThenInclude(c => c.Complaint)
                .AsQueryable();

            // Date range filters
            if (!string.IsNullOrWhiteSpace(request.DateFrom) &&
                DateTimeOffset.TryParse(request.DateFrom, out var dateFrom))
                query = query.Where(h => h.TransitionedAt >= dateFrom);

            if (!string.IsNullOrWhiteSpace(request.DateTo) &&
                DateTimeOffset.TryParse(request.DateTo, out var dateTo))
                query = query.Where(h => h.TransitionedAt <= dateTo);

            // Text search: case number, subject, or complaint reference
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var q = request.Search.ToLower();
                query = query.Where(h =>
                    h.Case.CaseNumber.Value.Contains(q) ||
                    h.Case.Subject.ToLower().Contains(q) ||
                    h.Case.Complaint.ReferenceNumber.ToLower().Contains(q));
            }

            var total = await query.CountAsync(cancellationToken);

            // Fetch paged history rows
            var rows = await query
                .OrderByDescending(h => h.TransitionedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Resolve operator names in a single extra query
            var userIds = rows
                .Where(h => h.ChangedByUserId != Guid.Empty)
                .Select(h => h.ChangedByUserId)
                .Distinct()
                .ToList();

            var users = userIds.Count > 0
                ? await _userRepo.Query()
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                    .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken)
                : new Dictionary<Guid, string>();

            var items = rows.Select(h => new CaseHistoryListDto(
                h.Id,
                h.CaseId,
                h.Case.CaseNumber.Value,
                h.Case.Subject,
                h.Case.Complaint.TaxType,
                $"Transitioned: {h.OldStatus} → {h.NewStatus}",
                h.NewStatus.ToString(),
                h.TransitionedAt,
                h.Reason,
                h.ChangedByUserId != Guid.Empty && users.TryGetValue(h.ChangedByUserId, out var name)
                    ? name
                    : "System"
            )).ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Case history retrieved.";
            response.Data = new PagedResult<CaseHistoryListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Document Upload (bulk) ────────────────────────────────────────────

    public async Task<Response<List<Guid>>> UploadCaseDocumentsAsync(UploadCaseDocumentsCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Guid>>();
        var ids = new List<Guid>();
        try
        {
            foreach (var file in request.Files)
            {
                await using var stream = file.OpenReadStream();
                var path = await _storage.StoreAsync(stream, file.FileName, file.ContentType, cancellationToken);

                var docId = Guid.NewGuid();
                var doc = new Document
                {
                    Id = docId, FileName = file.FileName, FilePath = path,
                    ContentType = file.ContentType, FileSize = file.Length,
                    EntityType = DocumentEntityType.Case, EntityId = request.CaseId,
                    CreatedAt = DateTime.UtcNow
                };
                await _docRepo.AddAsync(doc);
                ids.Add(docId);
            }
            await _docRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = $"{ids.Count} document(s) uploaded successfully.";
            response.Data = ids;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocUploadError;
        }
        return response;
    }
}
