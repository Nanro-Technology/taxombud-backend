using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class AdmissibilityAssessment : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    // 5 Screening Criteria
    public bool IsNotAnonymous { get; set; }
    public bool IsNotInCourt { get; set; }
    public bool IsWithinMandate { get; set; }
    public bool HasSupportingDocuments { get; set; }
    public bool HasExhaustedInternalProcedures { get; set; }

    public bool IsAdmissible { get; set; }
    public string? ScreeningNotes { get; set; }
    public string? RejectionReason { get; set; }

    public Guid AssessedByUserId { get; set; }
    public DateTimeOffset AssessedAt { get; set; } = DateTimeOffset.UtcNow;
}
