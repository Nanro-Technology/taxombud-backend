using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class AttendanceLog : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public int LateMinutes { get; set; }
    
    public string Status { get; set; } = "Present"; // Present, Absent, Half-Day, Late
    public string Source { get; set; } = "Web"; // Web, App, Biometric
}
