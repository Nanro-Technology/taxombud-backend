using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Strategies;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Commands;

public record SubmitCaseToWorkflowCommand(Guid CaseId, Guid? WorkflowId = null) : IRequest<WorkflowInstanceDto>;

public class SubmitCaseToWorkflowCommandHandler : IRequestHandler<SubmitCaseToWorkflowCommand, WorkflowInstanceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly RoutingStrategyFactory _strategyFactory;
    private readonly ICurrentUser _currentUser;

    public SubmitCaseToWorkflowCommandHandler(
        IApplicationDbContext context,
        RoutingStrategyFactory strategyFactory,
        ICurrentUser currentUser)
    {
        _context = context;
        _strategyFactory = strategyFactory;
        _currentUser = currentUser;
    }

    public async Task<WorkflowInstanceDto> Handle(SubmitCaseToWorkflowCommand request, CancellationToken cancellationToken)
    {
        var @case = await _context.Cases.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (@case == null)
        {
            throw new NotFoundException(nameof(Case), request.CaseId);
        }

        // 1. Resolve Workflow
        Workflow? workflow = null;
        if (request.WorkflowId.HasValue)
        {
            workflow = await _context.Workflows.Include(w => w.Levels).FirstOrDefaultAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);
        }

        if (workflow == null)
        {
            // Find default or category matching workflow
            workflow = await _context.Workflows.Include(w => w.Levels).FirstOrDefaultAsync(w => w.IsDefault && w.IsActive, cancellationToken)
                    ?? await _context.Workflows.Include(w => w.Levels).FirstOrDefaultAsync(w => w.IsActive, cancellationToken);
        }

        if (workflow == null || !workflow.Levels.Any())
        {
            throw new DomainException("No active workflow template with approval levels is configured.");
        }

        // 2. Resolve Latest Published Version
        var version = await _context.WorkflowVersions
            .Where(v => v.WorkflowId == workflow.Id && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version == null)
        {
            // Auto-publish version 1 if not explicitly published yet
            version = new WorkflowVersion(workflow.Id, 1, "{}");
            version.Publish(_currentUser.UserId ?? Guid.Empty);
            _context.WorkflowVersions.Add(version);
        }

        // 3. Create WorkflowInstance
        var instance = new WorkflowInstance(@case.Id, workflow.Id, version.Id);
        _context.WorkflowInstances.Add(instance);

        // 4. Create Instance Levels & Resolve Level 1 Assignee
        var sortedLevels = workflow.Levels.OrderBy(l => l.LevelNumber).ToList();
        WorkflowInstanceLevel? firstInstanceLevel = null;
        Guid? firstAssigneeId = null;

        foreach (var level in sortedLevels)
        {
            Guid? assignedUserId = null;
            if (level.LevelNumber == 1)
            {
                var strategy = _strategyFactory.GetStrategy(level.AssignmentAlgorithm);
                assignedUserId = await strategy.SelectAssigneeAsync(level.TargetRoleId, level.TargetUserId, cancellationToken);
                firstAssigneeId = assignedUserId;
            }

            var instanceLevel = new WorkflowInstanceLevel(
                instance.Id,
                level.Id,
                level.LevelNumber,
                assignedUserId,
                level.TargetRoleId,
                level.SlaHours,
                level.EscalationHours
            );

            if (level.LevelNumber == 1)
            {
                instanceLevel.Status = WorkflowLevelStatus.InProgress;
                firstInstanceLevel = instanceLevel;
            }

            _context.WorkflowInstanceLevels.Add(instanceLevel);
        }

        // 5. Create Level 1 CaseApprovalTask if assignee resolved
        if (firstInstanceLevel != null && firstAssigneeId.HasValue)
        {
            var task = new CaseApprovalTask(
                instance.Id,
                firstInstanceLevel.Id,
                @case.Id,
                firstAssigneeId.Value,
                firstInstanceLevel.AssignedRoleId
            );
            _context.CaseApprovalTasks.Add(task);
            instance.ApprovalTasks.Add(task);

            // Update Case assignment
            @case.Assign(firstAssigneeId.Value, _currentUser.UserId ?? Guid.Empty);
        }

        @case.ActiveWorkflowInstanceId = instance.Id;
        @case.UpdateStatus(CaseStatus.InProgress, sortedLevels.First().Name, _currentUser.UserId ?? Guid.Empty);

        // 6. Log Audit Entry
        var audit = new CaseWorkflowAuditLog(
            @case.Id,
            instance.Id,
            _currentUser.UserId ?? Guid.Empty,
            "System/User",
            "Submitted",
            "Open",
            "InProgress",
            1,
            sortedLevels.First().Name,
            "Case submitted to workflow engine",
            null,
            firstAssigneeId
        );
        _context.CaseWorkflowAuditLogs.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkflowInstanceDto(
            instance.Id,
            instance.CaseId,
            instance.WorkflowId,
            workflow.Name,
            instance.CurrentLevelNumber,
            instance.Status,
            instance.StartedAt,
            instance.CompletedAt,
            instance.ApprovalTasks.Select(t => new CaseApprovalTaskDto(
                t.Id,
                t.WorkflowInstanceId,
                t.WorkflowInstanceLevelId,
                t.CaseId,
                @case.Subject,
                t.AssignedUserId,
                "Assigned Officer",
                t.AssignedRoleId,
                null,
                t.Action,
                t.TaskStatus,
                t.Comment,
                t.PerformedAt,
                t.CreatedAt
            )).ToList()
        );
    }
}
