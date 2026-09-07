using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

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
        // Validate that at least one AdmissibilityGate level is present
        if (request.Levels == null || !request.Levels.Any(l => l.LevelRole == LevelRole.AdmissibilityGate))
        {
            throw new DomainException("Every workflow must include at least one level with the 'Admissibility Gate' stage role.");
        }

        if (request.IsDefault)
        {
            // Unset current default workflows of the same category
            var defaults = _context.Workflows.Where(w => w.IsDefault && w.CaseCategory == request.CaseCategory);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        var workflow = new Workflow(request.Name, request.Description, request.CaseCategory, request.IsDefault);

        foreach (var levelReq in request.Levels.OrderBy(l => l.LevelNumber))
        {
            var level = new WorkflowLevel(
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

            // Add multi-targets (roles + departments + specific users)
            if (levelReq.Targets != null)
            {
                foreach (var t in levelReq.Targets)
                {
                    level.Targets.Add(new WorkflowLevelTarget(level.Id, t.TargetType, t.TargetId));
                }
            }

            workflow.Levels.Add(level);
        }

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with target names for response
        return await BuildWorkflowDto(workflow, cancellationToken);
    }

    private async Task<WorkflowDto> BuildWorkflowDto(Workflow workflow, CancellationToken ct)
    {
        // Resolve display names for targets
        var roleIds = workflow.Levels.SelectMany(l => l.Targets)
            .Where(t => t.TargetType == WorkflowLevelTargetType.Role)
            .Select(t => t.TargetId).Distinct().ToList();

        var deptIds = workflow.Levels.SelectMany(l => l.Targets)
            .Where(t => t.TargetType == WorkflowLevelTargetType.Department)
            .Select(t => t.TargetId).Distinct().ToList();

        var userIds = workflow.Levels.SelectMany(l => l.Targets)
            .Where(t => t.TargetType == WorkflowLevelTargetType.User)
            .Select(t => t.TargetId).Distinct().ToList();

        var roleNames = roleIds.Any()
            ? await _context.CustomRoles.Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name ?? "Unknown Role", ct)
            : new Dictionary<Guid, string>();

        var deptNames = deptIds.Any()
            ? await _context.Departments.Where(d => deptIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name, ct)
            : new Dictionary<Guid, string>();

        var userNames = userIds.Any()
            ? await _context.Users.Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => (u.FirstName + " " + u.LastName).Trim(), ct)
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
