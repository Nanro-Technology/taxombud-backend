using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Reports.Queries.GetResolutionTimeReport;

public record GetResolutionTimeReportQuery(int? Year) : IRequest<Result<List<ResolutionTimeDto>>>;

public record ResolutionTimeDto(int Month, double AverageDays);

public class GetResolutionTimeReportQueryHandler : IRequestHandler<GetResolutionTimeReportQuery, Result<List<ResolutionTimeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetResolutionTimeReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResolutionTimeDto>>> Handle(GetResolutionTimeReportQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? global::System.DateTime.UtcNow.Year;

        var cases = await _context.Cases
            .Where(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue && c.ClosedAt.Value.Year == year)
            .ToListAsync(cancellationToken);

        var stats = cases
            .GroupBy(c => c.ClosedAt!.Value.Month)
            .Select(g => new ResolutionTimeDto(
                g.Key,
                g.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays)
            ))
            .OrderBy(s => s.Month)
            .ToList();

        return Result<List<ResolutionTimeDto>>.Success(stats);
    }
}
