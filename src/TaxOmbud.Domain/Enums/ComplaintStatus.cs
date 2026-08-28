namespace TaxOmbud.Domain.Enums;

public enum ComplaintStatus
{
    Submitted = 1,
    Registered = 2,
    UnderAssessment = 3,
    Assigned = 4,
    UnderInvestigation = 5,
    DecisionIssued = 6,
    Closed = 7,
    Draft = 8,
    Withdrawn = 9,

    /// <summary>
    /// Terminal status — complaint was declared Not Admissible at Stage 4.
    /// Taxpayer is formally notified with the reason.
    /// </summary>
    NotAdmissible = 10
}

