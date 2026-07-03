using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Appointments.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly IApplicationDbContext _context;

    public AppointmentsService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<BookAppointmentResponse>> BookAppointmentAsync(BookAppointmentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<BookAppointmentResponse>();
        try
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = AppointmentStatus.Scheduled,
                TaxpayerId = request.TaxpayerId,
                OfficerId = request.OfficerId,
                Location = request.Location,
                MeetingUrl = request.MeetingUrl
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Appointment booked successfully.";
            response.Data = new BookAppointmentResponse(
                appointment.Id,
                appointment.Title,
                appointment.Description,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Status.ToString(),
                appointment.TaxpayerId,
                appointment.OfficerId,
                appointment.Location,
                appointment.MeetingUrl
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while booking the appointment.";
            return response;
        }
    }

    public async Task<Response<object?>> UpdateAppointmentAsync(UpdateAppointmentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var entity = await _context.Appointments
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = $"Appointment '{request.Id}' was not found.";
                return response;
            }

            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.Location = request.Location;
            entity.MeetingUrl = request.MeetingUrl;

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Appointment updated successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the appointment.";
            return response;
        }
    }

    public async Task<Response<object?>> UpdateAppointmentStatusAsync(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var app = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);
            if (app == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Appointment not found.";
                return response;
            }

            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatus))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"Invalid status: '{request.Status}'.";
                return response;
            }

            app.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Appointment status updated successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the appointment status.";
            return response;
        }
    }

    public async Task<Response<AppointmentDetailDto>> GetAppointmentByIdAsync(GetAppointmentByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AppointmentDetailDto>();
        try
        {
            var app = await _context.Appointments
                .Include(a => a.Taxpayer)
                .Include(a => a.Officer!)
                    .ThenInclude(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (app == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Appointment not found.";
                return response;
            }

            var dto = new AppointmentDetailDto(
                app.Id,
                app.Title,
                app.Description,
                app.StartTime,
                app.EndTime,
                app.Status.ToString(),
                app.Taxpayer != null ? new AppointmentTaxpayerDto(app.Taxpayer.Id, app.Taxpayer.FirstName + " " + app.Taxpayer.LastName, app.Taxpayer.Email.Value) : null,
                app.Officer != null && app.Officer.User != null ? new AppointmentOfficerDto(app.Officer.Id, app.Officer.User.FullName, app.Officer.User.Email ?? string.Empty) : null,
                app.Location,
                app.MeetingUrl,
                app.CreatedAt,
                app.LastModifiedAt
            );

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Appointment retrieved successfully.";
            response.Data = dto;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the appointment.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<AppointmentListDto>>> GetAppointmentsAsync(GetAppointmentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<AppointmentListDto>>();
        try
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

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Appointments retrieved successfully.";
            response.Data = list;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the appointments.";
            return response;
        }
    }

    public async Task<Response<IReadOnlyList<TimeSlotDto>>> GetAvailabilityAsync(GetAvailabilityQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<TimeSlotDto>>();
        try
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
            for (int i = 9; i < 17; i++)
            {
                var slotStart = new DateTimeOffset(startOfDay.AddHours(i), request.Date.Offset);
                var slotEnd = slotStart.AddHours(1);

                var isBooked = existingAppointments.Any(a =>
                    (a.StartTime < slotEnd && a.EndTime > slotStart));

                slots.Add(new TimeSlotDto(slotStart, slotEnd, !isBooked));
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Availability retrieved successfully.";
            response.Data = slots;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving availability.";
            return response;
        }
    }

    public async Task<Response<IReadOnlyList<CalendarEventDto>>> GetCalendarAsync(GetCalendarQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CalendarEventDto>>();
        try
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

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Calendar retrieved successfully.";
            response.Data = appointments.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the calendar.";
            return response;
        }
    }
}
