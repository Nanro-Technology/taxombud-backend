using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Strategies;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace TaxOmbud.Application.Workflows.Commands;


public record ExecuteCaseApprovalCommand(
    Guid TaskId,
    WorkflowAction Action,
    string? Comment = null,
    Guid? AttachmentId = null,
    int? ReturnToLevelNumber = null
) : IRequest<bool>;

public class ExecuteCaseApprovalCommandHandler : IRequestHandler<ExecuteCaseApprovalCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly RoutingStrategyFactory _strategyFactory;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly ICaseWorkflowStageService _workflowStageService;
    private readonly ILogger<ExecuteCaseApprovalCommandHandler> _logger;

    public ExecuteCaseApprovalCommandHandler(
        IApplicationDbContext context,
        RoutingStrategyFactory strategyFactory,
        ICurrentUser currentUser,
        IEmailService emailService,
        ICaseWorkflowStageService workflowStageService,
        ILogger<ExecuteCaseApprovalCommandHandler> logger)
    {
        _context = context;
        _strategyFactory = strategyFactory;
        _currentUser = currentUser;
        _emailService = emailService;
        _workflowStageService = workflowStageService;
        _logger = logger;
    }


    public async Task<bool> Handle(ExecuteCaseApprovalCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CaseApprovalTasks
            .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.Workflow)
                    .ThenInclude(w => w.Levels)
                        .ThenInclude(l => l.Targets)  // Load multi-targets for routing
            .Include(t => t.WorkflowInstanceLevel)
                .ThenInclude(il => il.WorkflowLevel)
            .Include(t => t.Case)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(CaseApprovalTask), request.TaskId);
        }

        if (task.TaskStatus != WorkflowLevelStatus.Pending)
        {
            throw new DomainException("This approval task has already been executed.");
        }

        if (task.Case != null && task.Case.Status == CaseStatus.Closed)
        {
            throw new DomainException("This case is already closed. Workflow approval actions cannot be executed on closed cases.");
        }

        var levelConfig = task.WorkflowInstanceLevel.WorkflowLevel;

        // Validation rules
        if (levelConfig.RequireComment && string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new DomainException($"Comments are required for level '{levelConfig.Name}'.");
        }

        if (levelConfig.RequireAttachment && !request.AttachmentId.HasValue)
        {
            throw new DomainException($"An attachment is required for level '{levelConfig.Name}'.");
        }

        var currentUserId = _currentUser.UserId ?? Guid.Empty;
        var instance = task.WorkflowInstance;
        var @case = task.Case;
        var previousStatus = instance.Status.ToString();

        // Execute task action
        task.Execute(request.Action, request.Comment, request.AttachmentId);
        task.WorkflowInstanceLevel.Status = task.TaskStatus;
        task.WorkflowInstanceLevel.CompletedAt = DateTimeOffset.UtcNow;

        var allLevels = instance.Workflow.Levels.OrderBy(l => l.LevelNumber).ToList();
        var currentLevelNum = instance.CurrentLevelNumber;

        switch (request.Action)
        {
            case WorkflowAction.Approve:
                var nextLevel = allLevels.FirstOrDefault(l => l.LevelNumber > currentLevelNum);
                if (nextLevel == null)
                {
                    // Final Approval Reached -> Complete Workflow & Case
                    instance.Complete(WorkflowStatus.Approved);
                    @case.Close("Approved", request.Comment ?? "Approved via workflow engine", currentUserId);
                    _pendingClosureOutcome = "Approved";
                }
                else
                {
                    // Advance to Next Level
                    instance.AdvanceToLevel(nextLevel.LevelNumber);
                    var nextInstanceLevel = await _context.WorkflowInstanceLevels
                        .FirstOrDefaultAsync(il => il.WorkflowInstanceId == instance.Id && il.LevelNumber == nextLevel.LevelNumber, cancellationToken);

                    if (nextInstanceLevel != null)
                    {
                        nextInstanceLevel.Status = WorkflowLevelStatus.InProgress;

                        // Resolve assignee using multi-target strategy
                        var strategy = _strategyFactory.GetStrategy(nextLevel.AssignmentAlgorithm);
                        var (_, primaryRoleId) = await CandidateResolver.ResolveAsync(_context, nextLevel.Targets, cancellationToken);
                        var nextAssigneeId = await strategy.SelectAssigneeAsync(nextLevel.Targets, cancellationToken);

                        nextInstanceLevel.AssignedUserId = nextAssigneeId;
                        nextInstanceLevel.AssignedRoleId = primaryRoleId;

                        var nextTask = new CaseApprovalTask(
                            instance.Id,
                            nextInstanceLevel.Id,
                            @case.Id,
                            nextAssigneeId,
                            primaryRoleId
                        );
                        _context.CaseApprovalTasks.Add(nextTask);
                    }

                    // Use LevelRole to drive canonical stage/status — works for any N-level workflow
                    @case.UpdateStatus(GetCanonicalStatus(nextLevel.LevelRole), GetCanonicalStage(nextLevel.LevelRole), currentUserId);
                }
                break;

            case WorkflowAction.Reject:
                instance.Complete(WorkflowStatus.Rejected);
                @case.Close("Rejected", request.Comment ?? "Rejected via workflow engine", currentUserId);
                _pendingClosureOutcome = "Rejected";
                break;

            case WorkflowAction.ReturnForCorrection:
                var returnLevelNum = request.ReturnToLevelNumber ?? 1;
                var returnLevel = allLevels.FirstOrDefault(l => l.LevelNumber == returnLevelNum) ?? allLevels.First();
                instance.AdvanceToLevel(returnLevel.LevelNumber);

                var returnInstanceLevel = await _context.WorkflowInstanceLevels
                    .FirstOrDefaultAsync(il => il.WorkflowInstanceId == instance.Id && il.LevelNumber == returnLevel.LevelNumber, cancellationToken);

                if (returnInstanceLevel != null)
                {
                    returnInstanceLevel.Status = WorkflowLevelStatus.InProgress;

                    var strategy = _strategyFactory.GetStrategy(returnLevel.AssignmentAlgorithm);
                    var (_, returnPrimaryRoleId) = await CandidateResolver.ResolveAsync(_context, returnLevel.Targets, cancellationToken);
                    var returnAssigneeId = await strategy.SelectAssigneeAsync(returnLevel.Targets, cancellationToken);

                    returnInstanceLevel.AssignedUserId = returnAssigneeId;
                    returnInstanceLevel.AssignedRoleId = returnPrimaryRoleId;

                    var returnTask = new CaseApprovalTask(
                        instance.Id,
                        returnInstanceLevel.Id,
                        @case.Id,
                        returnAssigneeId,
                        returnPrimaryRoleId
                    );
                    _context.CaseApprovalTasks.Add(returnTask);
                }

                @case.UpdateStatus(CaseStatus.UnderInvestigation, $"Returned to {returnLevel.Name}", currentUserId);
                break;

            case WorkflowAction.Escalate:
                instance.Status = WorkflowStatus.Escalated;
                @case.UpdateStatus(CaseStatus.UnderInvestigation, $"Escalated at {levelConfig.Name}", currentUserId);
                break;
        }

        // Write Audit Log
        var audit = new CaseWorkflowAuditLog(
            @case.Id,
            instance.Id,
            currentUserId,
            "Officer",
            request.Action.ToString(),
            previousStatus,
            instance.Status.ToString(),
            currentLevelNum,
            levelConfig.Name,
            request.Comment,
            task.AssignedUserId,
            null
        );
        _context.CaseWorkflowAuditLogs.Add(audit);

        // Sync linked Complaint status so UI steppers & headers reflect the updated status immediately
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == @case.ComplaintId, cancellationToken);
        if (complaint != null)
        {
            complaint.UpdateStatus(@case.Status, @case.CurrentStage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // If this action caused a case closure, dispatch multi-recipient closure notifications
        if (!string.IsNullOrEmpty(_pendingClosureOutcome))
        {
            await _workflowStageService.SendCaseClosureNotificationsAsync(
                @case.Id,
                instance.Id,
                _pendingClosureOutcome,
                request.Comment,
                cancellationToken);
        }
        else
        {
            // For non-closure actions, send the standard audit copy to the acting officer
            try
            {
                var caseRef = @case.CaseNumber?.Value ?? @case.Id.ToString();
                var initiatorEmail = _currentUser.Email;
                if (!string.IsNullOrWhiteSpace(initiatorEmail))
                {
                    var auditHtml = $"""
                        <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                          <h3 style="color:#114a31;margin-top:0;">Audit Copy: Case Workflow Action Executed</h3>
                          <p>Hello <strong>{_currentUser.FullName ?? "Officer"}</strong>,</p>
                          <p>You executed workflow action <strong>{request.Action}</strong> at level <strong>{levelConfig.Name}</strong> for Case Reference <strong>{caseRef}</strong>.</p>
                          <p><strong>Workflow Status:</strong> Transitioned from <em>{previousStatus}</em> to <em>{instance.Status}</em>.</p>
                        </div>
                        """;
                    await _emailService.SendAsync(initiatorEmail, $"[Audit Copy] Case {caseRef}: Workflow Action {request.Action}", auditHtml, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send workflow action audit-copy email for task {TaskId}", request.TaskId);
            }
        }

        return true;

    }

    // Transient flag used within a single Handle() call to signal a closure outcome for post-save notifications
    private string? _pendingClosureOutcome;

    /// <summary>
    /// Maps a LevelRole to a canonical WorkflowStage string.
    /// Works for any N-level workflow — not position-dependent.
    /// </summary>
    private static string GetCanonicalStage(LevelRole levelRole) => levelRole switch
    {
        LevelRole.Intake            => WorkflowStage.Intake,
        LevelRole.Registration      => WorkflowStage.RegistrationAndAcknowledgement,
        LevelRole.InitialReview     => WorkflowStage.InitialReviewAndAssignment,
        LevelRole.AdmissibilityGate => WorkflowStage.JurisdictionAndAdmissibility,
        LevelRole.Investigation     => WorkflowStage.InvestigationAndResolution,
        LevelRole.Decision          => WorkflowStage.DecisionAndCommunication,
        LevelRole.Closure           => WorkflowStage.ClosureAndArchiving,
        _                           => WorkflowStage.Intake
    };

    /// <summary>
    /// Maps a LevelRole to a canonical CaseStatus.
    /// Works for any N-level workflow — not position-dependent.
    /// </summary>
    private static CaseStatus GetCanonicalStatus(LevelRole levelRole) => levelRole switch
    {
        LevelRole.Intake            => CaseStatus.Submitted,
        LevelRole.Registration      => CaseStatus.Registered,
        LevelRole.InitialReview     => CaseStatus.UnderAssessment,
        LevelRole.AdmissibilityGate => CaseStatus.UnderAssessment,
        LevelRole.Investigation     => CaseStatus.UnderInvestigation,
        LevelRole.Decision          => CaseStatus.DecisionIssued,
        LevelRole.Closure           => CaseStatus.Closed,
        _                           => CaseStatus.UnderInvestigation
    };
}
