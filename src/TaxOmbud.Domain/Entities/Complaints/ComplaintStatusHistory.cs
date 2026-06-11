using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Complaints;

public class ComplaintStatusHistory : BaseEntity
{
    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public ComplaintStatus OldStatus { get; set; }
    public ComplaintStatus NewStatus { get; set; }
    
    public Guid ChangedByUserId { get; set; }
    public DateTimeOffset TransitionedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string? Reason { get; set; }
}
