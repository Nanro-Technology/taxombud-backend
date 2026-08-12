using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;

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
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workflow == null)
        {
            throw new NotFoundException(nameof(Workflow), request.Id);
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

        // Upsert levels in-place to preserve primary keys and avoid foreign key constraint violations
        if (request.Levels != null && request.Levels.Any())
        {
            var requestedLevels = request.Levels.OrderBy(l => l.LevelNumber).ToList();
            var requestedLevelNumbers = requestedLevels.Select(l => l.LevelNumber).ToHashSet();

            // 1. Remove levels that are no longer in the request (only if not referenced by workflow instances)
            var levelsToRemove = workflow.Levels.Where(l => !requestedLevelNumbers.Contains(l.LevelNumber)).ToList();
            foreach (var lvlToRemove in levelsToRemove)
            {
                var isReferenced = await _context.WorkflowInstanceLevels.AnyAsync(il => il.WorkflowLevelId == lvlToRemove.Id, cancellationToken);
                if (!isReferenced)
                {
                    _context.WorkflowLevels.Remove(lvlToRemove);
                    workflow.Levels.Remove(lvlToRemove);
                }
            }

            // 2. Update existing levels in-place or add new levels
            foreach (var levelReq in requestedLevels)
            {
                var existingLevel = workflow.Levels.FirstOrDefault(l => l.LevelNumber == levelReq.LevelNumber);
                if (existingLevel != null)
                {
                    existingLevel.Name = levelReq.Name;
                    existingLevel.Description = levelReq.Description;
                    existingLevel.SlaHours = levelReq.SlaHours;
                    existingLevel.EscalationHours = levelReq.EscalationHours;
                    existingLevel.IsMandatory = levelReq.IsMandatory;
                    existingLevel.RequireComment = levelReq.RequireComment;
                    existingLevel.RequireAttachment = levelReq.RequireAttachment;
                    existingLevel.TargetType = levelReq.TargetType;
                    existingLevel.TargetRoleId = levelReq.TargetRoleId;
                    existingLevel.TargetUserId = levelReq.TargetUserId;
                    existingLevel.AssignmentMode = levelReq.AssignmentMode;
                    existingLevel.AssignmentAlgorithm = levelReq.AssignmentAlgorithm;
                }
                else
                {
                    var newLevel = new WorkflowLevel(
                        workflow.Id,
                        levelReq.LevelNumber,
                        levelReq.Name,
                        levelReq.Description,
                        levelReq.TargetType,
                        levelReq.TargetRoleId,
                        levelReq.TargetUserId,
                        levelReq.AssignmentMode,
                        levelReq.AssignmentAlgorithm
                    )
                    {
                        SlaHours = levelReq.SlaHours,
                        EscalationHours = levelReq.EscalationHours,
                        IsMandatory = levelReq.IsMandatory,
                        RequireComment = levelReq.RequireComment,
                        RequireAttachment = levelReq.RequireAttachment
                    };
                    workflow.Levels.Add(newLevel);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkflowDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.CaseCategory,
            workflow.IsActive,
            workflow.IsDefault,
            workflow.CurrentVersion,
            workflow.CreatedAt,
            workflow.Levels.Select(l => new WorkflowLevelDto(
                l.Id,
                l.LevelNumber,
                l.Name,
                l.Description,
                l.SlaHours,
                l.EscalationHours,
                l.IsMandatory,
                l.RequireComment,
                l.RequireAttachment,
                l.TargetType,
                l.TargetRoleId,
                null,
                l.TargetUserId,
                null,
                l.AssignmentMode,
                l.AssignmentAlgorithm
            )).ToList()
        );
    }
}
