namespace TaxOmbud.Domain.Enums;

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
