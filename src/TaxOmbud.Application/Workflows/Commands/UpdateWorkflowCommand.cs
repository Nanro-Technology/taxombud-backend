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

        // Clear existing levels and rebuild
        if (workflow.Levels.Any())
        {
            _context.WorkflowLevels.RemoveRange(workflow.Levels);
            workflow.Levels.Clear();
        }

        if (request.Levels != null && request.Levels.Any())
        {
            foreach (var levelReq in request.Levels.OrderBy(l => l.LevelNumber))
            {
                var level = new WorkflowLevel(
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
                workflow.Levels.Add(level);
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
