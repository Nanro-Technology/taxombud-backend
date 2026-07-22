using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;

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
        )).ToList();
    }
}
