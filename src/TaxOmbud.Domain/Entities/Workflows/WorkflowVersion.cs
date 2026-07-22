using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Workflows;

public class WorkflowVersion : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string SnapshotJson { get; set; } = null!; // Full JSON snapshot of levels and settings
    public bool IsPublished { get; set; } = false;
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }

    protected WorkflowVersion() { }

    public WorkflowVersion(Guid workflowId, int versionNumber, string snapshotJson)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        VersionNumber = versionNumber;
        SnapshotJson = snapshotJson;
        CreatedAt = DateTime.UtcNow;
    }

    public void Publish(Guid publishedByUserId)
    {
        IsPublished = true;
        PublishedAt = DateTimeOffset.UtcNow;
        PublishedByUserId = publishedByUserId;
    }
}
