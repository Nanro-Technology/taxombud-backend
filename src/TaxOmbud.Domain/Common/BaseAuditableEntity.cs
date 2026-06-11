using System;

namespace TaxOmbud.Domain.Common;

/// <summary>
/// Extends BaseEntity with full audit fields (who created/updated, and when).
/// BaseEntity already has CreatedAt and UpdatedAt timestamps.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
}
