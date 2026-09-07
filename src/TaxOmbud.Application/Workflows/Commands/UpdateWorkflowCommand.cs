using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Commands;

public record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string Description,
    string CaseCategory = "General",
    bool IsDefault = false,
    List<CreateWorkflowLevelRequest>? Levels = null
) : IRequest<WorkflowDto>;

public class UpdateWorkflowCommandHandler : IRequestHandler<UpdateWorkflowCommand, WorkflowDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkflowCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowDto> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Levels)
                .ThenInclude(l => l.Targets)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workflow == null)
        {
            throw new NotFoundException(nameof(Workflow), request.Id);
        }

        // Validate mandatory AdmissibilityGate
        if (request.Levels == null || !request.Levels.Any(l => l.LevelRole == LevelRole.AdmissibilityGate))
        {
            throw new DomainException("Every workflow must include at least one level with the 'Admissibility Gate' stage role.");
        }

        if (request.IsDefault)
        {
            var defaults = _context.Workflows.Where(w => w.IsDefault && w.CaseCategory == request.CaseCategory && w.Id != request.Id);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        workflow.Name = request.Name;
        workflow.Description = request.Description;
        workflow.CaseCategory = request.CaseCategory;
        workflow.IsDefault = request.IsDefault;

        // Upsert levels in-place to preserve PKs
        var requestedLevels = request.Levels.OrderBy(l => l.LevelNumber).ToList();
        var requestedLevelNumbers = requestedLevels.Select(l => l.LevelNumber).ToHashSet();

        // Remove levels no longer in request (only if not referenced by workflow instances)
        var levelsToRemove = workflow.Levels.Where(l => !requestedLevelNumbers.Contains(l.LevelNumber)).ToList();
        foreach (var lvlToRemove in levelsToRemove)
        {
            var isReferenced = await _context.WorkflowInstanceLevels
                .AnyAsync(il => il.WorkflowLevelId == lvlToRemove.Id, cancellationToken);
            if (!isReferenced)
            {
                _context.WorkflowLevels.Remove(lvlToRemove);
                workflow.Levels.Remove(lvlToRemove);
            }
        }

        // Update existing levels or add new ones
        foreach (var levelReq in requestedLevels)
        {
            var existingLevel = workflow.Levels.FirstOrDefault(l => l.LevelNumber == levelReq.LevelNumber);
            if (existingLevel != null)
            {
                existingLevel.Name = levelReq.Name;
                existingLevel.Description = levelReq.Description;
                existingLevel.LevelRole = levelReq.LevelRole;
                existingLevel.SlaHours = levelReq.SlaHours;
                existingLevel.EscalationHours = levelReq.EscalationHours;
                existingLevel.IsMandatory = levelReq.IsMandatory;
                existingLevel.RequireComment = levelReq.RequireComment;
                existingLevel.RequireAttachment = levelReq.RequireAttachment;
                existingLevel.AssignmentMode = levelReq.AssignmentMode;
                existingLevel.AssignmentAlgorithm = levelReq.AssignmentAlgorithm;

                // Replace targets: remove old, add new
                var existingTargets = existingLevel.Targets.ToList();
                foreach (var t in existingTargets)
                {
                    _context.WorkflowLevelTargets.Remove(t);
                }
                existingLevel.Targets.Clear();

                if (levelReq.Targets != null)
                {
                    foreach (var t in levelReq.Targets)
                    {
                        existingLevel.Targets.Add(new WorkflowLevelTarget(existingLevel.Id, t.TargetType, t.TargetId));
                    }
                }
            }
            else
            {
                var newLevel = new WorkflowLevel(
                    workflow.Id,
                    levelReq.LevelNumber,
                    levelReq.Name,
                    levelReq.Description,
                    levelReq.LevelRole,
                    levelReq.AssignmentMode,
                    levelReq.AssignmentAlgorithm)
                {
                    SlaHours = levelReq.SlaHours,
                    EscalationHours = levelReq.EscalationHours,
                    IsMandatory = levelReq.IsMandatory,
                    RequireComment = levelReq.RequireComment,
                    RequireAttachment = levelReq.RequireAttachment
                };

                if (levelReq.Targets != null)
                {
                    foreach (var t in levelReq.Targets)
                    {
                        newLevel.Targets.Add(new WorkflowLevelTarget(newLevel.Id, t.TargetType, t.TargetId));
                    }
                }

                workflow.Levels.Add(newLevel);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload targets for accurate response
        await _context.WorkflowLevelTargets
            .Where(t => workflow.Levels.Select(l => l.Id).Contains(t.WorkflowLevelId))
            .LoadAsync(cancellationToken);

        // Resolve display names
        var allTargets = workflow.Levels.SelectMany(l => l.Targets).ToList();
        var roleIds = allTargets.Where(t => t.TargetType == WorkflowLevelTargetType.Role).Select(t => t.TargetId).Distinct().ToList();
        var deptIds = allTargets.Where(t => t.TargetType == WorkflowLevelTargetType.Department).Select(t => t.TargetId).Distinct().ToList();
        var userIds = allTargets.Where(t => t.TargetType == WorkflowLevelTargetType.User).Select(t => t.TargetId).Distinct().ToList();

        var roleNames = roleIds.Any()
            ? await _context.CustomRoles.Where(r => roleIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.Name ?? "Unknown Role", cancellationToken)
            : new Dictionary<Guid, string>();
        var deptNames = deptIds.Any()
            ? await _context.Departments.Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken)
            : new Dictionary<Guid, string>();
        var userNames = userIds.Any()
            ? await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => (u.FirstName + " " + u.LastName).Trim(), cancellationToken)
            : new Dictionary<Guid, string>();

        return new WorkflowDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.CaseCategory,
            workflow.IsActive,
            workflow.IsDefault,
            workflow.CurrentVersion,
            workflow.CreatedAt,
            workflow.Levels.OrderBy(l => l.LevelNumber).Select(l => new WorkflowLevelDto(
                l.Id,
                l.LevelNumber,
                l.Name,
                l.Description,
                l.LevelRole,
                l.SlaHours,
                l.EscalationHours,
                l.IsMandatory,
                l.RequireComment,
                l.RequireAttachment,
                l.AssignmentMode,
                l.AssignmentAlgorithm,
                l.Targets.Select(t => new WorkflowLevelTargetDto(
                    t.Id,
                    t.TargetType,
                    t.TargetId,
                    t.TargetType switch
                    {
                        WorkflowLevelTargetType.Role       => roleNames.GetValueOrDefault(t.TargetId, "Unknown Role"),
                        WorkflowLevelTargetType.Department => deptNames.GetValueOrDefault(t.TargetId, "Unknown Dept"),
                        WorkflowLevelTargetType.User       => userNames.GetValueOrDefault(t.TargetId, "Unknown User"),
                        _                                  => "Unknown"
                    }
                )).ToList()
            )).ToList()
        );
    }
}
