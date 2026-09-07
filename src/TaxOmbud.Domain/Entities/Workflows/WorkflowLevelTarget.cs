using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

/// <summary>
/// Junction entity that represents a single assignment target for a WorkflowLevel.
/// A level may have multiple targets of mixed types (Role + Department + User).
/// The routing engine intersects Department and Role memberships to determine
/// who can see and act on the generated CaseApprovalTask.
/// Specific User targets bypass the role/department filter and are always eligible.
/// </summary>
public class WorkflowLevelTarget : BaseEntity
{
    public Guid WorkflowLevelId { get; set; }
    public WorkflowLevel WorkflowLevel { get; set; } = null!;

    /// <summary>Role = 1, Department = 2, User = 3</summary>
    public WorkflowLevelTargetType TargetType { get; set; }

    /// <summary>
    /// Points to Role.Id, Department.Id, or User.Id depending on TargetType.
    /// Not a typed FK to allow multi-type references without polymorphic joins.
    /// </summary>
    public Guid TargetId { get; set; }

    protected WorkflowLevelTarget() { }

    public WorkflowLevelTarget(Guid workflowLevelId, WorkflowLevelTargetType targetType, Guid targetId)
    {
        Id = Guid.NewGuid();
        WorkflowLevelId = workflowLevelId;
        TargetType = targetType;
        TargetId = targetId;
        CreatedAt = DateTime.UtcNow;
    }
}
