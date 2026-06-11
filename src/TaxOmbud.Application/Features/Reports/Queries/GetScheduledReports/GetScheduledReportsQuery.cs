using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetScheduledReports;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetScheduledReportsQuery() : IRequest<Result<IEnumerable<ScheduledReportDto>>>;

public record ScheduledReportDto(
    Guid Id,
    string ReportName,
    string CronExpression,
    string Recipients,
    string Format,
    bool IsActive,
    DateTimeOffset? LastRunAt,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetScheduledReportsQueryHandler : IRequestHandler<GetScheduledReportsQuery, Result<IEnumerable<ScheduledReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetScheduledReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ScheduledReportDto>>> Handle(GetScheduledReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _context.ScheduledReports
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ScheduledReportDto(
                r.Id,
                r.ReportName,
                r.CronExpression,
                r.Recipients,
                r.Format,
                r.IsActive,
                r.LastRunAt,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ScheduledReportDto>>.Success(reports);
    }
}
