using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetOfficerWorkload;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetOfficerWorkloadQuery() : IRequest<Result<IEnumerable<OfficerWorkloadDto>>>;

public record OfficerWorkloadDto(
    Guid OfficerProfileId,
    int ActiveCases
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetOfficerWorkloadQueryHandler : IRequestHandler<GetOfficerWorkloadQuery, Result<IEnumerable<OfficerWorkloadDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOfficerWorkloadQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<OfficerWorkloadDto>>> Handle(GetOfficerWorkloadQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.OfficerCaseloads
            .Include(c => c.OfficerProfile)
                .ThenInclude(o => o.User)
            .Where(c => c.IsActive)
            .GroupBy(c => c.OfficerProfileId)
            .Select(g => new OfficerWorkloadDto(
                g.Key,
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<OfficerWorkloadDto>>.Success(data);
    }
}
