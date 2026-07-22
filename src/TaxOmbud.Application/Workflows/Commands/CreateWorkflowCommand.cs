using MediatR;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Application.Workflows.Commands;

public record CreateWorkflowCommand(
    string Name,
    string Description,
    string CaseCategory = "General",
    bool IsDefault = false,
    List<CreateWorkflowLevelRequest>? Levels = null
) : IRequest<WorkflowDto>;

public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkflowCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            // Unset current default workflows if this one is set as default
            var defaults = _context.Workflows.Where(w => w.IsDefault && w.CaseCategory == request.CaseCategory);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        var workflow = new Workflow(request.Name, request.Description, request.CaseCategory, request.IsDefault);

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

        _context.Workflows.Add(workflow);
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
