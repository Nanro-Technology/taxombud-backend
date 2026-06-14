using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PerformanceCycle : BaseAuditableEntity
{
    public string Name { get; set; } = null!; // e.g. Q1 2026
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Active, Closed
}

public class PerformanceGoal : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid CycleId { get; set; }
    public PerformanceCycle Cycle { get; set; } = null!;
    
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public int ProgressPercentage { get; set; } = 0;
    public string Status { get; set; } = "Not Started";
}

public class PerformanceReview : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid CycleId { get; set; }
    public PerformanceCycle Cycle { get; set; } = null!;
    
    public decimal Score { get; set; }
    public string? ReviewerNotes { get; set; }
    public string? EmployeeComments { get; set; }
    
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Acknowledged
}
