namespace TaxOmbud.Application.Cases.DTOs;

// ── Stage Transition Commands ───────────────────────────────────────────────

/// <summary>Stage 3 — CE triggers Initial Review and assigns to a department/officer.</summary>
public record StartInitialReviewCommand(
    Guid CaseId,
    Guid? AssignedOfficerId,
    Guid? DepartmentId
);

/// <summary>
/// Stage 4 terminal — Officer declares the complaint Not Admissible.
/// A formal notification email is automatically dispatched to the taxpayer.
/// </summary>
public record DeclareNotAdmissibleCommand(
    Guid CaseId,
    string Reason,
    Guid AdmissibilityAssessmentId
);

/// <summary>Stage 7 — Registry officer archives the closed case.</summary>
public record ArchiveCaseCommand(
    Guid CaseId,
    string ArchivePurpose,
    List<string> ArchivedDocumentRefs
);

// ── Response DTOs ───────────────────────────────────────────────────────────

public class NotAdmissibleDecisionDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string Reason { get; set; } = null!;
    public string DeclaredByName { get; set; } = null!;
    public DateTimeOffset DeclaredAt { get; set; }
    public bool NotificationSent { get; set; }
    public DateTimeOffset? NotificationSentAt { get; set; }
}

public class CaseArchiveRecordDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string ArchivePurpose { get; set; } = null!;
    public List<string> ArchivedDocumentRefs { get; set; } = new();
    public string ArchivedByName { get; set; } = null!;
    public DateTimeOffset ArchivedAt { get; set; }
    public string? Notes { get; set; }
}
