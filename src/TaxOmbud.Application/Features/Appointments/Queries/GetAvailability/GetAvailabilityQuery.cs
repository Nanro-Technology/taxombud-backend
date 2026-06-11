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

namespace TaxOmbud.Application.Features.Appointments.Queries.GetAvailability;

public record GetAvailabilityQuery(Guid OfficerId, DateTimeOffset Date)
    : IRequest<Result<IReadOnlyList<TimeSlotDto>>>;

public record TimeSlotDto(DateTimeOffset StartTime, DateTimeOffset EndTime, bool IsAvailable);

public class GetAvailabilityQueryHandler
    : IRequestHandler<GetAvailabilityQuery, Result<IReadOnlyList<TimeSlotDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailabilityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<TimeSlotDto>>> Handle(
        GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var startOfDay = request.Date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var existingAppointments = await _context.Appointments
            .Where(a => a.OfficerId == request.OfficerId 
                     && a.StartTime >= startOfDay 
                     && a.EndTime < endOfDay
                     && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var slots = new List<TimeSlotDto>();
        // Generate hourly slots from 9 AM to 5 PM
        for (int i = 9; i < 17; i++)
        {
            var slotStart = new DateTimeOffset(startOfDay.AddHours(i), request.Date.Offset);
            var slotEnd = slotStart.AddHours(1);

            var isBooked = existingAppointments.Any(a => 
                (a.StartTime < slotEnd && a.EndTime > slotStart));

            slots.Add(new TimeSlotDto(slotStart, slotEnd, !isBooked));
        }

        return Result<IReadOnlyList<TimeSlotDto>>.Success(slots);
    }
}
