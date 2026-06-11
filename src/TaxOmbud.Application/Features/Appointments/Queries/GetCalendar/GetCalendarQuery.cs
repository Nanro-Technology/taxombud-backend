using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Appointments.Queries.GetCalendar;

public record GetCalendarQuery(Guid? OfficerId, Guid? TaxpayerId, int Month, int Year)
    : IRequest<Result<IReadOnlyList<CalendarEventDto>>>;

public record CalendarEventDto(
    Guid Id,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status
);

public class GetCalendarQueryHandler
    : IRequestHandler<GetCalendarQuery, Result<IReadOnlyList<CalendarEventDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCalendarQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CalendarEventDto>>> Handle(
        GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddMonths(1);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.StartTime >= startDate && a.StartTime < endDate);

        if (request.OfficerId.HasValue)
            query = query.Where(a => a.OfficerId == request.OfficerId.Value);

        if (request.TaxpayerId.HasValue)
            query = query.Where(a => a.TaxpayerId == request.TaxpayerId.Value);

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .Select(a => new CalendarEventDto(
                a.Id,
                a.Title,
                a.StartTime,
                a.EndTime,
                a.Status.ToString()
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CalendarEventDto>>.Success(appointments.AsReadOnly());
    }
}
