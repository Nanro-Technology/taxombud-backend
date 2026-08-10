using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Strategies;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

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

    public ExecuteCaseApprovalCommandHandler(
        IApplicationDbContext context,
        RoutingStrategyFactory strategyFactory,
        ICurrentUser currentUser)
    {
        _context = context;
        _strategyFactory = strategyFactory;
        _currentUser = currentUser;
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
        return true;
    }
}
