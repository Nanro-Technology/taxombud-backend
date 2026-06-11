using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseStatusHistory : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public CaseStatus OldStatus { get; set; }
    public CaseStatus NewStatus { get; set; }
    
    public Guid ChangedByUserId { get; set; }
    public DateTimeOffset TransitionedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string? Reason { get; set; }
}
