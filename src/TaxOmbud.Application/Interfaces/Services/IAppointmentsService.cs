using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Appointments.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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
