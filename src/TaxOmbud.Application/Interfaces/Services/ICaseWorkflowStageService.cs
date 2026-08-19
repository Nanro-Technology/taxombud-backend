using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public class AdmissibilityAssessmentDto
{
    public bool IsNotAnonymous { get; set; }
    public bool IsNotInCourt { get; set; }
    public bool IsWithinMandate { get; set; }
    public bool HasSupportingDocuments { get; set; }
    public bool HasExhaustedInternalProcedures { get; set; }
    public bool IsAdmissible { get; set; }
    public string? ScreeningNotes { get; set; }
    public string? RejectionReason { get; set; }
}

public class MediationLogDto
{
    public DateTimeOffset SessionDate { get; set; }
    public string Attendees { get; set; } = null!;
    public string SummaryOfDiscussions { get; set; } = null!;
    public string? SettlementProposal { get; set; }
    public bool IsAmicablySettled { get; set; }
    public string? AgreementDocumentUrl { get; set; }
}

public class QualityAssuranceReviewDto
{
    public bool AccuracyVerified { get; set; }
    public bool ConsistencyVerified { get; set; }
    public bool LegalComplianceVerified { get; set; }
    public bool PolicyAdherenceVerified { get; set; }
    public bool IsApprovedForDecision { get; set; }
    public string QaComments { get; set; } = null!;
    public string? RevisionInstructions { get; set; }
}

public class CaseDecisionDto
{
    public string DecisionSummary { get; set; } = null!;
    public string LegalBasisCitations { get; set; } = null!;
    public string RecommendationsApproved { get; set; } = null!;
    public string? DecisionDocumentUrl { get; set; }
    public string IssuerTitle { get; set; } = "Chief Executive";
}

public class CallCenterRecordDto
{
    public Guid ComplaintId { get; set; }
    public string CallerName { get; set; } = null!;
    public string CallerPhoneNumber { get; set; } = null!;
    public string HotlineLineUsed { get; set; } = null!;
    public int DurationSeconds { get; set; }
    public string? RecordingFileUrl { get; set; }
    public string CallSummary { get; set; } = null!;
}

/// <summary>New DTO for case recommendations (Gap 4).</summary>
public class CaseRecommendationDto
{
    public Guid Id { get; set; }
    public string RecommendationText { get; set; } = null!;
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
}

public class WorkflowStageDetailsDto
{
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = null!;
    public string CurrentStage { get; set; } = null!;
    public string? CurrentSubStage { get; set; }
    public string Status { get; set; } = null!;
    public AdmissibilityAssessmentDto? Admissibility { get; set; }
    public MediationLogDto[] MediationSessions { get; set; } = Array.Empty<MediationLogDto>();
    public CaseFindingDto[] Findings { get; set; } = Array.Empty<CaseFindingDto>();
    public CaseRecommendationDto[] Recommendations { get; set; } = Array.Empty<CaseRecommendationDto>();
    public QualityAssuranceReviewDto[] QaReviews { get; set; } = Array.Empty<QualityAssuranceReviewDto>();
    public CaseDecisionDto? Decision { get; set; }
    public CallCenterRecordDto[] CallRecords { get; set; } = Array.Empty<CallCenterRecordDto>();
}

public interface ICaseWorkflowStageService
{
    Task<bool> RegisterComplaintAsync(Guid complaintId, Guid registeredBy);
    Task<bool> AssessAdmissibilityAsync(Guid caseId, AdmissibilityAssessmentDto dto, Guid assessedBy);
    Task<bool> AssignCaseByCeAsync(Guid caseId, Guid officerId, Guid departmentId, Guid assignedBy);
    Task<bool> LogMediationSessionAsync(Guid caseId, MediationLogDto dto, Guid loggedBy);
    Task<bool> SubmitQaReviewAsync(Guid caseId, QualityAssuranceReviewDto dto, Guid reviewedBy);
    Task<bool> IssueCeDecisionAsync(Guid caseId, CaseDecisionDto dto, Guid issuedBy);
    Task<bool> CloseAndArchiveCaseAsync(Guid caseId, string outcome, string summary, Guid closedBy);
    Task<Guid> LogCallCenterRecordAsync(CallCenterRecordDto dto, Guid loggedBy);
    Task<WorkflowStageDetailsDto?> GetWorkflowStageDetailsAsync(Guid caseId);

    /// <summary>
    /// Sends case-closure email notifications to all relevant parties:
    /// 1. The complaint lodger (taxpayer user who filed the complaint).
    /// 2. Every officer who acted on an approval task during the workflow.
    /// 3. For corporate complaints (TaxpayerType != Individual), the same lodger email
    ///    is used but the subject and body prominently reference the organisation name (Option C).
    /// </summary>
    Task SendCaseClosureNotificationsAsync(
        Guid caseId,
        Guid workflowInstanceId,
        string outcome,
        string? finalComment,
        CancellationToken cancellationToken = default);
}