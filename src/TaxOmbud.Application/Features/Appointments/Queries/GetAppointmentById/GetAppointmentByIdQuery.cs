using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Appointments.Queries.GetAppointmentById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDetailDto>>;

public record AppointmentDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    AppointmentTaxpayerDto? Taxpayer,
    AppointmentOfficerDto? Officer,
    string? Location,
    string? MeetingUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record AppointmentTaxpayerDto(Guid Id, string FullName, string Email);
public record AppointmentOfficerDto(Guid Id, string FullName, string Email);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDetailDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await _context.Appointments
            .Include(a => a.Taxpayer)
            .Include(a => a.Officer!)
                .ThenInclude(o => o.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (app == null)
            return Result<AppointmentDetailDto>.NotFound("Appointment not found.");

        var dto = new AppointmentDetailDto(
            app.Id,
            app.Title,
            app.Description,
            app.StartTime,
            app.EndTime,
            app.Status.ToString(),
            app.Taxpayer != null ? new AppointmentTaxpayerDto(app.Taxpayer.Id, app.Taxpayer.FirstName + " " + app.Taxpayer.LastName, app.Taxpayer.Email.Value) : null,
            app.Officer != null && app.Officer.User != null ? new AppointmentOfficerDto(app.Officer.Id, app.Officer.User.FullName, app.Officer.User.Email) : null,
            app.Location,
            app.MeetingUrl,
            app.CreatedAt,
            app.UpdatedAt
        );

        return Result<AppointmentDetailDto>.Success(dto);
    }
}
