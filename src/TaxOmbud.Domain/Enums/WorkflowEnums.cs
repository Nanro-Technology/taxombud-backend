namespace TaxOmbud.Domain.Enums;

/// <summary>
/// Semantic role of a workflow level. Drives Case.CurrentStage and CaseStatus
/// independently of the level's position number, enabling N-level workflows.
/// Every workflow MUST contain at least one level with LevelRole.AdmissibilityGate.
/// </summary>
public enum LevelRole
{
    Custom = 0,              // User-defined / blank stage
    Intake = 1,              // Stage 1 — complaint received
    Registration = 2,        // Stage 2 — CRN assigned, acknowledgement sent
    InitialReview = 3,       // Stage 3 — CE review & assignment
    AdmissibilityGate = 4,   // Stage 4 — jurisdiction & admissibility (MANDATORY, conditional branch)
    Investigation = 5,       // Stage 5 — investigation & resolution
    Decision = 6,            // Stage 6 — formal decision & communication
    Closure = 7              // Stage 7 — closure & archiving
}

/// <summary>
/// Identifies the kind of target stored in a WorkflowLevelTarget row.
/// Multiple targets of any combination can exist for a single WorkflowLevel.
/// Routing engine intersects Department + Role memberships; specific Users bypass that filter.
/// </summary>
public enum WorkflowLevelTargetType
{
    Role = 1,
    Department = 2,
    User = 3
}

public enum WorkflowStatus
{
    Draft = 1,
    Submitted = 2,
    PendingApproval = 3,
    InProgress = 4,
    Returned = 5,
    Rejected = 6,
    Approved = 7,
    Completed = 8,
    Cancelled = 9,
    Escalated = 10,
    Reassigned = 11
}

public enum AssignmentTargetType
{
    User = 1,
    Role = 2,
    UserAndRole = 3
}

public enum AssignmentMode
{
    Manual = 1,
    Automatic = 2
}

public enum AssignmentAlgorithm
{
    RoundRobin = 1,
    LeastWorkload = 2,
    Random = 3,
    FirstAvailable = 4,
    LowestActiveCases = 5,
    Custom = 6
}

public enum WorkflowLevelStatus
{
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Returned = 5,
    Skipped = 6,
    Escalated = 7
}

public enum WorkflowAction
{
    Approve = 1,
    Reject = 2,
    ReturnForCorrection = 3,
    Reassign = 4,
    Skip = 5,
    Escalate = 6
}
