using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Cases;

/// <summary>Officer notes/progress updates on a case.</summary>
public class CaseNote : BaseAuditableEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Content { get; set; } = null!;
    public bool IsInternal { get; set; } = true;  // Internal notes not visible to taxpayer
}
