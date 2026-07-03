using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class DisciplinaryCase : BaseEntity
{
    public string CaseReference { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Guid? HrOfficerId { get; set; }
    
    public string IncidentType { get; set; } = null!;
    public DateTime IncidentDate { get; set; }
    public DateTime? HearingDate { get; set; }
    
    public string Description { get; set; } = null!;
    public string? ActionTaken { get; set; }
    public string? Outcome { get; set; }
    
    public string Status { get; set; } = "Open"; // Open, Under Investigation, Closed
    public bool IsConfidential { get; set; } = false;
}

public class ExitRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string ExitType { get; set; } = "Resignation"; // Resignation, Termination, Retirement
    
    public DateTime NoticeDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public DateTime? ExitDate { get; set; }
    
    public string Reason { get; set; } = null!;
    
    public Guid? HandoverToEmployeeId { get; set; }
    public string? HandoverNotes { get; set; }
    
    public string Status { get; set; } = "Pending"; // Pending, Approved, Completed
    public Guid? ApprovedById { get; set; }
}
