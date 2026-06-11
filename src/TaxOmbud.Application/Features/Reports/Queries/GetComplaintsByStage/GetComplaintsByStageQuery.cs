using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByStage;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetComplaintsByStageQuery() : IRequest<Result<IEnumerable<ComplaintsByStageDto>>>;

public record ComplaintsByStageDto(
    string Stage,
    int Count
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintsByStageQueryHandler : IRequestHandler<GetComplaintsByStageQuery, Result<IEnumerable<ComplaintsByStageDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsByStageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ComplaintsByStageDto>>> Handle(GetComplaintsByStageQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Complaints
            .GroupBy(c => c.CurrentStage)
            .Select(g => new ComplaintsByStageDto(
                g.Key,
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ComplaintsByStageDto>>.Success(data);
    }
}
