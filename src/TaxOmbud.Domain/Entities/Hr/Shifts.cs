using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class Shift : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    public int GracePeriodMinutes { get; set; } = 15;
    public int BreakMinutes { get; set; } = 60;
    public int OvertimeThresholdMinutes { get; set; } = 30;
    
    public bool IsActive { get; set; } = true;
}

public class ShiftAssignment : BaseAuditableEntity
{
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;

    public Guid TargetId { get; set; } // Can be UserId or DepartmentId
    public string TargetType { get; set; } = "User"; // User, Department
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
