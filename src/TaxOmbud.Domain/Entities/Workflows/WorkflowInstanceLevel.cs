using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

public class WorkflowInstanceLevel : BaseEntity
{
    public Guid WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public Guid WorkflowLevelId { get; set; }
    public WorkflowLevel WorkflowLevel { get; set; } = null!;

    public int LevelNumber { get; set; }
    public WorkflowLevelStatus Status { get; set; } = WorkflowLevelStatus.Pending;

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public Guid? AssignedRoleId { get; set; }
    public Role? AssignedRole { get; set; }

    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? EscalatesAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    protected WorkflowInstanceLevel() { }

    public WorkflowInstanceLevel(Guid workflowInstanceId, Guid workflowLevelId, int levelNumber, Guid? assignedUserId, Guid? assignedRoleId, int? slaHours, int? escalationHours)
    {
        Id = Guid.NewGuid();
        WorkflowInstanceId = workflowInstanceId;
        WorkflowLevelId = workflowLevelId;
        LevelNumber = levelNumber;
        Status = WorkflowLevelStatus.Pending;
        AssignedUserId = assignedUserId;
        AssignedRoleId = assignedRoleId;
        CreatedAt = DateTime.UtcNow;

        if (slaHours.HasValue && slaHours.Value > 0)
        {
            DueAt = DateTimeOffset.UtcNow.AddHours(slaHours.Value);
        }

        if (escalationHours.HasValue && escalationHours.Value > 0)
        {
            EscalatesAt = DateTimeOffset.UtcNow.AddHours(escalationHours.Value);
        }
    }
}
