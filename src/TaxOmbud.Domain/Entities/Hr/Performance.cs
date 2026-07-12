using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PerformanceCycle : BaseEntity
{
    public string Name { get; set; } = null!; // e.g. Q1 2026
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Active, Closed
}

public class PerformanceGoal : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid CycleId { get; set; }
    public PerformanceCycle Cycle { get; set; } = null!;
    
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public int ProgressPercentage { get; set; } = 0;
    public string Status { get; set; } = "Not Started";
}

public class PerformanceReview : BaseEntity
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

public class Competency : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int SortOrder { get; set; } = 1;
    public string Status { get; set; } = "Active"; // Active, Inactive
}

public class ReviewTemplate : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int QuestionCount { get; set; } = 5;
    public string Status { get; set; } = "Active"; // Active, Inactive
}
