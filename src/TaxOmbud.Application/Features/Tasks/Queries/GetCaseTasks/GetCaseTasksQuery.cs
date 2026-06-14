using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Tasks.DTOs;

namespace TaxOmbud.Application.Features.Tasks.Queries.GetCaseTasks;

public record GetCaseTasksQuery : IRequest<List<CaseTaskDto>>;

public class GetCaseTasksQueryHandler : IRequestHandler<GetCaseTasksQuery, List<CaseTaskDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CaseTaskDto>> Handle(GetCaseTasksQuery request, CancellationToken cancellationToken)
    {
        return await _context.CaseTasks
            .AsNoTracking()
            .Select(x => new CaseTaskDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                DueAt = x.DueAt,
                AssignedToId = x.AssignedToId,
                LinkedCaseId = x.LinkedCaseId,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
