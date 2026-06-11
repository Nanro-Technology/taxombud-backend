using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.System.Queries.GetFeatureFlags;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetFeatureFlagsQuery() : IRequest<Result<IEnumerable<FeatureFlag>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetFeatureFlagsQueryHandler : IRequestHandler<GetFeatureFlagsQuery, Result<IEnumerable<FeatureFlag>>>
{
    private readonly IApplicationDbContext _context;

    public GetFeatureFlagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<FeatureFlag>>> Handle(GetFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        var flags = await _context.FeatureFlags.AsNoTracking().ToListAsync(cancellationToken);
        return Result<IEnumerable<FeatureFlag>>.Success(flags);
    }
}
