using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class AdmissibilityAssessment : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    // Screening Criteria (Stage 4 — Jurisdiction & Admissibility Assessment)
    public bool IsNotAnonymous { get; set; }
    public bool IsNotInCourt { get; set; }
    public bool IsWithinMandate { get; set; }
    public bool HasSupportingDocuments { get; set; }
    public bool HasExhaustedInternalProcedures { get; set; }

    /// <summary>Jurisdiction check — confirms the Tax Ombud has authority over this matter.</summary>
    public bool JurisdictionCheck { get; set; }

    public bool IsAdmissible { get; set; }

    /// <summary>
    /// Computed outcome label for display purposes.
    /// Returns "ADMISSIBLE" or "NOT ADMISSIBLE" based on IsAdmissible.
    /// </summary>
    public string OutcomeLabel => IsAdmissible ? "ADMISSIBLE" : "NOT ADMISSIBLE";

    public string? ScreeningNotes { get; set; }
    public string? RejectionReason { get; set; }

    public Guid AssessedByUserId { get; set; }
    public DateTimeOffset AssessedAt { get; set; } = DateTimeOffset.UtcNow;
}

