using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

public class WorkflowInstance : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    public Guid WorkflowVersionId { get; set; }
    public WorkflowVersion WorkflowVersion { get; set; } = null!;

    public int CurrentLevelNumber { get; set; } = 1;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Submitted;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<WorkflowInstanceLevel> InstanceLevels { get; set; } = new List<WorkflowInstanceLevel>();
    public ICollection<CaseApprovalTask> ApprovalTasks { get; set; } = new List<CaseApprovalTask>();

    protected WorkflowInstance() { }

    public WorkflowInstance(Guid caseId, Guid workflowId, Guid workflowVersionId)
    {
        Id = Guid.NewGuid();
        CaseId = caseId;
        WorkflowId = workflowId;
        WorkflowVersionId = workflowVersionId;
        CurrentLevelNumber = 1;
        Status = WorkflowStatus.Submitted;
        StartedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void AdvanceToLevel(int levelNumber)
    {
        CurrentLevelNumber = levelNumber;
        Status = WorkflowStatus.InProgress;
    }

    public void Complete(WorkflowStatus finalStatus)
    {
        Status = finalStatus;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
