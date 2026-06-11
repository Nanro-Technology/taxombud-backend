using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseMilestone : BaseAuditableEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public DateTimeOffset? TargetDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    
    public bool IsCompleted { get; set; }
}
