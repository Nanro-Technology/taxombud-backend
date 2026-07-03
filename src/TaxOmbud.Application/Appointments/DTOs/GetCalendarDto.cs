namespace TaxOmbud.Application.Appointments.DTOs;

public record GetCalendarQuery(Guid? OfficerId, Guid? TaxpayerId, int Month, int Year) ;

public record CalendarEventDto(Guid Id, string Title, DateTimeOffset StartTime, DateTimeOffset EndTime, string Status);
