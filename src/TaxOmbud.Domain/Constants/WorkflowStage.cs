namespace TaxOmbud.Domain.Constants;

/// <summary>
/// Approved 7-stage case workflow stage slug constants.
/// These are the canonical stage identifiers stored in Cases.CurrentStage
/// and Complaints.CurrentStage. All stage transitions must use these constants.
/// </summary>
public static class WorkflowStage
{
    /// <summary>Stage 1 — Complaint received via any intake channel.</summary>
    public const string Intake = "1_intake";

    /// <summary>Stage 2 — Complaint formally registered; Case Reference Number assigned; acknowledgement sent.</summary>
    public const string RegistrationAndAcknowledgement = "2_registration";

    /// <summary>Stage 3 — CE performs initial review and assigns to an officer/department.</summary>
    public const string InitialReviewAndAssignment = "3_initial_review";

    /// <summary>Stage 4 — Assigned officer performs jurisdiction and admissibility screening.</summary>
    public const string JurisdictionAndAdmissibility = "4_admissibility";

    /// <summary>Terminal branch off Stage 4 — Complaint declared not admissible; auto-notification dispatched.</summary>
    public const string NotAdmissible = "4_not_admissible";

    /// <summary>Stage 5 — Full investigation and resolution. Includes mediation, findings, QA sub-activities.</summary>
    public const string InvestigationAndResolution = "5_investigation";

    /// <summary>Stage 6 — CE issues formal decision; Decision Letter PDF generated and communicated.</summary>
    public const string DecisionAndCommunication = "6_decision";

    /// <summary>Stage 7 — Case formally closed and records archived.</summary>
    public const string ClosureAndArchiving = "7_closure";
}
