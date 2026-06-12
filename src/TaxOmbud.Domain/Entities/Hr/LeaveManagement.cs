using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class LeaveTypeEntity : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public int DefaultDays { get; set; }
    public bool IsPaid { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class LeaveBalance : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeEntityId { get; set; }
    public LeaveTypeEntity LeaveTypeEntity { get; set; } = null!;
    
    public int Year { get; set; }
    public int AllottedDays { get; set; }
    public int UsedDays { get; set; }
}
