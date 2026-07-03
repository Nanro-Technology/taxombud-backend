namespace TaxOmbud.Application.HrRequests.DTOs;

public record SubmitLeaveRequestCommands(Guid StaffId, string LeaveType, DateTime StartDate, DateTime EndDate, string Reason) ;
