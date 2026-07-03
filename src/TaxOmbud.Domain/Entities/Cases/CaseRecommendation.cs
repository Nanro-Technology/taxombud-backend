using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseRecommendation : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string RecommendationText { get; set; } = null!;
    public string Status { get; set; } = "pending"; // pending, approved, rejected
    public Guid? ApprovedByUserId { get; set; }
    public string? Notes { get; set; }
}
