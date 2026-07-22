using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Workflows;

public class CaseWorkflowAuditLog : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public Guid WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public Guid PerformedByUserId { get; set; }
    public User PerformedByUser { get; set; } = null!;

    public string UserRole { get; set; } = null!;
    public string Action { get; set; } = null!; // Submitted, Approved, Rejected, Returned, Reassigned, Escalated
    public string PreviousStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;

    public int LevelNumber { get; set; }
    public string LevelName { get; set; } = null!;

    public Guid? PreviousAssigneeId { get; set; }
    public Guid? NewAssigneeId { get; set; }

    public string? Comment { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    protected CaseWorkflowAuditLog() { }

    public CaseWorkflowAuditLog(
        Guid caseId, 
        Guid workflowInstanceId, 
        Guid performedByUserId, 
        string userRole, 
        string action, 
        string previousStatus, 
        string newStatus, 
        int levelNumber, 
        string levelName, 
        string? comment, 
        Guid? previousAssigneeId = null, 
        Guid? newAssigneeId = null, 
        string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        CaseId = caseId;
        WorkflowInstanceId = workflowInstanceId;
        PerformedByUserId = performedByUserId;
        UserRole = userRole;
        Action = action;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        LevelNumber = levelNumber;
        LevelName = levelName;
        Comment = comment;
        PreviousAssigneeId = previousAssigneeId;
        NewAssigneeId = newAssigneeId;
        IpAddress = ipAddress;
        Timestamp = DateTimeOffset.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}
