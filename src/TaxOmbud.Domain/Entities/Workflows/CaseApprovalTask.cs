using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

public class CaseApprovalTask : BaseEntity
{
    public Guid WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public Guid WorkflowInstanceLevelId { get; set; }
    public WorkflowInstanceLevel WorkflowInstanceLevel { get; set; } = null!;

    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public Guid AssignedUserId { get; set; }
    public User AssignedUser { get; set; } = null!;

    public Guid? AssignedRoleId { get; set; }
    public Role? AssignedRole { get; set; }

    public WorkflowAction Action { get; set; } = WorkflowAction.Approve;
    public WorkflowLevelStatus TaskStatus { get; set; } = WorkflowLevelStatus.Pending;

    public string? Comment { get; set; }
    public Guid? AttachmentId { get; set; }
    public DateTimeOffset? PerformedAt { get; set; }

    protected CaseApprovalTask() { }

    public CaseApprovalTask(Guid workflowInstanceId, Guid workflowInstanceLevelId, Guid caseId, Guid assignedUserId, Guid? assignedRoleId)
    {
        Id = Guid.NewGuid();
        WorkflowInstanceId = workflowInstanceId;
        WorkflowInstanceLevelId = workflowInstanceLevelId;
        CaseId = caseId;
        AssignedUserId = assignedUserId;
        AssignedRoleId = assignedRoleId;
        TaskStatus = WorkflowLevelStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Execute(WorkflowAction action, string? comment, Guid? attachmentId)
    {
        Action = action;
        Comment = comment;
        AttachmentId = attachmentId;
        PerformedAt = DateTimeOffset.UtcNow;

        TaskStatus = action switch
        {
            WorkflowAction.Approve => WorkflowLevelStatus.Approved,
            WorkflowAction.Reject => WorkflowLevelStatus.Rejected,
            WorkflowAction.ReturnForCorrection => WorkflowLevelStatus.Returned,
            WorkflowAction.Skip => WorkflowLevelStatus.Skipped,
            WorkflowAction.Escalate => WorkflowLevelStatus.Escalated,
            _ => WorkflowLevelStatus.Pending
        };
    }
}
