using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class LeaveRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string LeaveType { get; set; } = null!; // Annual, Sick, Maternity, Casual, Study
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int Days { get; set; }
    
    public string? Reason { get; set; }

    public string Status { get; set; } = "pending"; // pending, approved, rejected, cancelled
    public Guid? ApproverUserId { get; set; }
    public User? ApproverUser { get; set; }
    public string? SupervisorNote { get; set; }
}
