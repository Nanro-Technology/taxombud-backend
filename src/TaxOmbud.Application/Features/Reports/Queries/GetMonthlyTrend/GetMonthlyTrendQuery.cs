using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetMonthlyTrend;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetMonthlyTrendQuery(int? Year) : IRequest<Result<MonthlyTrendResponseDto>>;

public record MonthlyTrendResponseDto(
    int Year,
    IEnumerable<MonthlyTrendDto> Monthly
);

public record MonthlyTrendDto(
    int Month,
    int Count
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetMonthlyTrendQueryHandler : IRequestHandler<GetMonthlyTrendQuery, Result<MonthlyTrendResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MonthlyTrendResponseDto>> Handle(GetMonthlyTrendQuery request, CancellationToken cancellationToken)
    {
        var targetYear = request.Year ?? DateTime.UtcNow.Year;
        var data = await _context.Complaints
            .Where(c => c.CreatedAt.Year == targetYear)
            .GroupBy(c => c.CreatedAt.Month)
            .Select(g => new MonthlyTrendDto(
                g.Key,
                g.Count()
            ))
            .OrderBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var response = new MonthlyTrendResponseDto(targetYear, data);
        return Result<MonthlyTrendResponseDto>.Success(response);
    }
}
