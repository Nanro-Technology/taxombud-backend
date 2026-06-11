using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.System.Queries.GetSettings;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetSettingsQuery() : IRequest<Result<IEnumerable<SystemSetting>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, Result<IEnumerable<SystemSetting>>>
{
    private readonly IApplicationDbContext _context;

    public GetSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<SystemSetting>>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.SystemSettings.AsNoTracking().ToListAsync(cancellationToken);
        return Result<IEnumerable<SystemSetting>>.Success(settings);
    }
}
