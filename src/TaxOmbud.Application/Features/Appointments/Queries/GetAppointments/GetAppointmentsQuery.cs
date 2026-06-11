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

namespace TaxOmbud.Application.Features.Appointments.Queries.GetAppointments;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAppointmentsQuery(
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Status
) : IRequest<Result<IEnumerable<AppointmentListDto>>>;

public record AppointmentListDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    string TaxpayerName,
    string OfficerName,
    string? Location,
    string? MeetingUrl
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<IEnumerable<AppointmentListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<AppointmentListDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Include(a => a.Taxpayer)
            .Include(a => a.Officer!)
                .ThenInclude(o => o.User)
            .AsNoTracking()
            .AsQueryable();

        if (request.TaxpayerId.HasValue)
            query = query.Where(a => a.TaxpayerId == request.TaxpayerId.Value);

        if (request.OfficerId.HasValue)
            query = query.Where(a => a.OfficerId == request.OfficerId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppointmentStatus>(request.Status, true, out var appStatus))
        {
            query = query.Where(a => a.Status == appStatus);
        }

        var list = await query
            .OrderByDescending(a => a.StartTime)
            .Select(a => new AppointmentListDto(
                a.Id,
                a.Title,
                a.Description,
                a.StartTime,
                a.EndTime,
                a.Status.ToString(),
                a.Taxpayer != null ? a.Taxpayer.FirstName + " " + a.Taxpayer.LastName : "Unknown",
                a.Officer != null && a.Officer.User != null ? a.Officer.User.FullName : "Unassigned",
                a.Location,
                a.MeetingUrl
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<AppointmentListDto>>.Success(list);
    }
}
