using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class QualityAssuranceReview : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public bool AccuracyVerified { get; set; }
    public bool ConsistencyVerified { get; set; }
    public bool LegalComplianceVerified { get; set; }
    public bool PolicyAdherenceVerified { get; set; }

    public bool IsApprovedForDecision { get; set; }
    public string QaComments { get; set; } = null!;
    public string? RevisionInstructions { get; set; }

    public Guid ReviewedByUserId { get; set; }
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;
}
