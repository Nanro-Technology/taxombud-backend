using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class AttendanceRecord : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    
    public DateTime Date { get; set; }
    
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    
    public string Status { get; set; } = "Present"; // Present, Absent, Late
    
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public int LateMinutes { get; set; }
    
    public string? Notes { get; set; }
}
