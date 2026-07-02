using TaxOmbud.Application.Appointments.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAppointmentsService
{
    Task<Response<BookAppointmentResponse>> BookAppointmentAsync(BookAppointmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateAppointmentAsync(UpdateAppointmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateAppointmentStatusAsync(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<AppointmentDetailDto>> GetAppointmentByIdAsync(GetAppointmentByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<AppointmentListDto>>> GetAppointmentsAsync(GetAppointmentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<TimeSlotDto>>> GetAvailabilityAsync(GetAvailabilityQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<CalendarEventDto>>> GetCalendarAsync(GetCalendarQuery request, CancellationToken cancellationToken = default);
}
