using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetWorkflowsQuery(string? Category = null, bool? IsActive = null) : IRequest<List<WorkflowDto>>;

public class GetWorkflowsQueryHandler : IRequestHandler<GetWorkflowsQuery, List<WorkflowDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkflowsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowDto>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Workflows
            .Include(w => w.Levels)
                .ThenInclude(l => l.Targets)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(w => w.CaseCategory == request.Category);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(w => w.IsActive == request.IsActive.Value);
        }

        var list = await query.ToListAsync(cancellationToken);

        // Collect all target IDs for name resolution (one query each type)
        var allTargets = list.SelectMany(w => w.Levels.SelectMany(l => l.Targets)).ToList();

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

        return list.Select(w => new WorkflowDto(
            w.Id,
            w.Name,
            w.Description,
            w.CaseCategory,
            w.IsActive,
            w.IsDefault,
            w.CurrentVersion,
            w.CreatedAt,
            w.Levels.OrderBy(l => l.LevelNumber).Select(l => new WorkflowLevelDto(
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
        )).ToList();
    }
}
