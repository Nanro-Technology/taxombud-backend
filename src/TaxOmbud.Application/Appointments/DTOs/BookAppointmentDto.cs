namespace TaxOmbud.Application.Appointments.DTOs;

public record BookAppointmentCommand(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
) ;

public record BookAppointmentResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);

public record BookAppointmentRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);
