namespace TaxOmbud.Application.Appointments.DTOs;

public record UpdateAppointmentCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Location,
    string? MeetingUrl
) ;

public record UpdateAppointmentRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Location,
    string? MeetingUrl
);
