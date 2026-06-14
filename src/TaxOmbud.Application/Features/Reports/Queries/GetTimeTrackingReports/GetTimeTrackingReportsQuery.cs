using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetTimeTrackingReports;

public class GetTimeTrackingReportsQuery : ReportFilterDto, IRequest<TimeTrackingReportDto> { }

public class GetTimeTrackingReportsQueryHandler : IRequestHandler<GetTimeTrackingReportsQuery, TimeTrackingReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTimeTrackingReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TimeTrackingReportDto> Handle(GetTimeTrackingReportsQuery request, CancellationToken cancellationToken)
    {
        var logsQuery = _context.TimeLogs.AsQueryable();

        if (request.StartDate.HasValue)
            logsQuery = logsQuery.Where(t => t.StartTime >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            logsQuery = logsQuery.Where(t => t.StartTime <= request.EndDate.Value);

        var now = DateTimeOffset.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

        var totalThisWeek = await _context.TimeLogs
            .Where(t => t.StartTime >= startOfWeek)
            .SumAsync(t => t.DurationHours, cancellationToken);

        var totalThisMonth = await _context.TimeLogs
            .Where(t => t.StartTime >= startOfMonth)
            .SumAsync(t => t.DurationHours, cancellationToken);

        var hoursByAgent = await logsQuery
            .Include(t => t.User)
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, User = g.FirstOrDefault()!.User, TotalHours = g.Sum(t => t.DurationHours) })
            .ToListAsync(cancellationToken);

        return new TimeTrackingReportDto
        {
            TotalHoursLoggedThisWeek = totalThisWeek,
            TotalHoursLoggedThisMonth = totalThisMonth,
            HoursByAgent = hoursByAgent.ToDictionary(k => $"{k.User?.FirstName} {k.User?.LastName}", v => v.TotalHours)
        };
    }
}
