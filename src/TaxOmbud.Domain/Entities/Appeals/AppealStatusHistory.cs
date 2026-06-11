using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Appeals;

public class AppealStatusHistory : BaseEntity
{
    public Guid AppealId { get; set; }
    public Appeal Appeal { get; set; } = null!;

    public AppealStatus OldStatus { get; set; }
    public AppealStatus NewStatus { get; set; }
    
    public Guid ChangedByUserId { get; set; }
    public DateTimeOffset TransitionedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string? Reason { get; set; }
}
