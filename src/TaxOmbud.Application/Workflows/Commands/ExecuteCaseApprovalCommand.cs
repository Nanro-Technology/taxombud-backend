using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Strategies;
using TaxOmbud.Common.CustomException;
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
    private readonly ILogger<ExecuteCaseApprovalCommandHandler> _logger;

    public ExecuteCaseApprovalCommandHandler(
        IApplicationDbContext context,
        RoutingStrategyFactory strategyFactory,
        ICurrentUser currentUser,
        IEmailService emailService,
        ILogger<ExecuteCaseApprovalCommandHandler> logger)
    {
        _context = context;
        _strategyFactory = strategyFactory;
        _currentUser = currentUser;
        _emailService = emailService;
        _logger = logger;
    }


    public async Task<bool> Handle(ExecuteCaseApprovalCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CaseApprovalTasks
            .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.Workflow)
                    .ThenInclude(w => w.Levels)
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

                        // Resolve assignee for next level using Strategy
                        var strategy = _strategyFactory.GetStrategy(nextLevel.AssignmentAlgorithm);
                        var nextAssigneeId = await strategy.SelectAssigneeAsync(nextLevel.TargetRoleId, nextLevel.TargetUserId, cancellationToken);

                        if (nextAssigneeId.HasValue)
                        {
                            nextInstanceLevel.AssignedUserId = nextAssigneeId;
                            var nextTask = new CaseApprovalTask(
                                instance.Id,
                                nextInstanceLevel.Id,
                                @case.Id,
                                nextAssigneeId.Value,
                                nextLevel.TargetRoleId
                            );
                            _context.CaseApprovalTasks.Add(nextTask);
                            @case.Assign(nextAssigneeId.Value, currentUserId);
                        }
                    }

                    @case.UpdateStatus(CaseStatus.UnderInvestigation, nextLevel.Name, currentUserId);
                }
                break;

            case WorkflowAction.Reject:
                instance.Complete(WorkflowStatus.Rejected);
                @case.Close("Rejected", request.Comment ?? "Rejected via workflow engine", currentUserId);
                break;

            case WorkflowAction.ReturnForCorrection:
                var returnLevelNum = request.ReturnToLevelNumber ?? 1;
                var returnLevel = allLevels.FirstOrDefault(l => l.LevelNumber == returnLevelNum) ?? allLevels.First();
                
                instance.CurrentLevelNumber = returnLevel.LevelNumber;
                instance.Status = WorkflowStatus.Returned;

                var returnInstanceLevel = await _context.WorkflowInstanceLevels
                    .FirstOrDefaultAsync(il => il.WorkflowInstanceId == instance.Id && il.LevelNumber == returnLevel.LevelNumber, cancellationToken);

                if (returnInstanceLevel != null)
                {
                    returnInstanceLevel.Status = WorkflowLevelStatus.InProgress;
                    var strategy = _strategyFactory.GetStrategy(returnLevel.AssignmentAlgorithm);
                    var returnAssigneeId = await strategy.SelectAssigneeAsync(returnLevel.TargetRoleId, returnLevel.TargetUserId, cancellationToken);

                    if (returnAssigneeId.HasValue)
                    {
                        var returnTask = new CaseApprovalTask(
                            instance.Id,
                            returnInstanceLevel.Id,
                            @case.Id,
                            returnAssigneeId.Value,
                            returnLevel.TargetRoleId
                        );
                        _context.CaseApprovalTasks.Add(returnTask);
                        @case.Assign(returnAssigneeId.Value, currentUserId);
                    }
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

        await _context.SaveChangesAsync(cancellationToken);

        // Dispatch audit notification copy to Initiator
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
            _logger.LogError(ex, "Failed to send workflow action email notification copy for task {TaskId}", request.TaskId);
        }

        return true;

    }
}
