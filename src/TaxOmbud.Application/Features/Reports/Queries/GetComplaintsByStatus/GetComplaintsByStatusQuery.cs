using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByStatus;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetComplaintsByStatusQuery() : IRequest<Result<IEnumerable<ComplaintsByStatusDto>>>;

public record ComplaintsByStatusDto(
    string Status,
    int Count
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintsByStatusQueryHandler : IRequestHandler<GetComplaintsByStatusQuery, Result<IEnumerable<ComplaintsByStatusDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsByStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ComplaintsByStatusDto>>> Handle(GetComplaintsByStatusQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new ComplaintsByStatusDto(
                g.Key.ToString(),
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ComplaintsByStatusDto>>.Success(data);
    }
}
