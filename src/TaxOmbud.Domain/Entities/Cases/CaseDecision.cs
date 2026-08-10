using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseDecision : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string DecisionSummary { get; set; } = null!;
    public string LegalBasisCitations { get; set; } = null!;
    public string RecommendationsApproved { get; set; } = null!;
    public string? DecisionDocumentUrl { get; set; }

    public Guid IssuedByUserId { get; set; } // Chief Executive or Authorized Officer
    public string IssuerTitle { get; set; } = "Chief Executive";
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
}
