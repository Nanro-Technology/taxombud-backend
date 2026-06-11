using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.PayGrades.Queries.GetSalaryProfiles;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetSalaryProfilesQuery(Guid? UserId) : IRequest<Result<IEnumerable<SalaryProfileDto>>>;

public record SalaryProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    decimal Basic,
    string? Allowances,
    string? Deductions,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetSalaryProfilesQueryHandler : IRequestHandler<GetSalaryProfilesQuery, Result<IEnumerable<SalaryProfileDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryProfilesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<SalaryProfileDto>>> Handle(GetSalaryProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalaryProfiles
            .Include(s => s.User)
            .AsNoTracking()
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(s => s.UserId == request.UserId.Value);

        var items = await query
            .OrderByDescending(s => s.EffectiveFrom)
            .Select(s => new SalaryProfileDto(
                s.Id,
                s.UserId,
                s.User.FullName,
                s.Basic,
                s.Allowances,
                s.Deductions,
                s.EffectiveFrom,
                s.EffectiveTo,
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<SalaryProfileDto>>.Success(items);
    }
}
