namespace TaxOmbud.Application.Appointments.DTOs;

public record GetAvailabilityQuery(Guid OfficerId, DateTimeOffset Date) ;

public record TimeSlotDto(DateTimeOffset StartTime, DateTimeOffset EndTime, bool IsAvailable);