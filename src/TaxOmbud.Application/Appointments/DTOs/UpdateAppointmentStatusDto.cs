namespace TaxOmbud.Application.Appointments.DTOs;

public record UpdateAppointmentStatusCommand(Guid AppointmentId, string Status) ;

public record UpdateAppointmentStatusRequest(string Status);
