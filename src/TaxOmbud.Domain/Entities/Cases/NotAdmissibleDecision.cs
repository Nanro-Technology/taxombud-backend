using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

/// <summary>
/// Records the formal Not Admissible declaration made at Stage 4 (Jurisdiction & Admissibility Assessment).
/// A formal notification email is dispatched to the taxpayer after this record is created.
/// </summary>
public class NotAdmissibleDecision : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    /// <summary>Formal reason why the complaint was declared not admissible.</summary>
    public string Reason { get; set; } = null!;

    /// <summary>Reference to the admissibility assessment that led to this decision.</summary>
    public Guid? AdmissibilityAssessmentId { get; set; }

    /// <summary>Officer who declared the complaint not admissible.</summary>
    public Guid DeclaredByUserId { get; set; }

    public DateTimeOffset DeclaredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Whether the formal notification email has been dispatched to the taxpayer.</summary>
    public bool NotificationSent { get; set; } = false;

    public DateTimeOffset? NotificationSentAt { get; set; }
}
