using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseTask : BaseAuditableEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public string Status { get; set; } = "Open"; // Open, In Progress, Closed
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
    
    public DateTimeOffset? DueAt { get; set; }
    
    public Guid? AssignedToId { get; set; }
    
    public Guid? LinkedCaseId { get; set; }
    public Case? LinkedCase { get; set; }
}
