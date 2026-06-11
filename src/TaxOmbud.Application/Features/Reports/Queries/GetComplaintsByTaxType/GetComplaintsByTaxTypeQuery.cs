using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByTaxType;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetComplaintsByTaxTypeQuery() : IRequest<Result<IEnumerable<ComplaintsByTaxTypeDto>>>;

public record ComplaintsByTaxTypeDto(
    string TaxType,
    int Count
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintsByTaxTypeQueryHandler : IRequestHandler<GetComplaintsByTaxTypeQuery, Result<IEnumerable<ComplaintsByTaxTypeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsByTaxTypeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ComplaintsByTaxTypeDto>>> Handle(GetComplaintsByTaxTypeQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Complaints
            .GroupBy(c => c.TaxType)
            .Select(g => new ComplaintsByTaxTypeDto(
                g.Key,
                g.Count()
            ))
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ComplaintsByTaxTypeDto>>.Success(data);
    }
}
