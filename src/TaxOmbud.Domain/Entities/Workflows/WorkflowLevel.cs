using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

public class WorkflowLevel : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    public int LevelNumber { get; set; } // 1-based order
    public string Name { get; set; } = null!; // e.g. "Level 1 - Loan Officer"
    public string? Description { get; set; }

    public int? SlaHours { get; set; }
    public int? EscalationHours { get; set; }

    public bool IsMandatory { get; set; } = true;
    public bool RequireComment { get; set; } = false;
    public bool RequireAttachment { get; set; } = false;

    public AssignmentTargetType TargetType { get; set; } = AssignmentTargetType.Role;
    public Guid? TargetRoleId { get; set; }
    public Role? TargetRole { get; set; }

    public Guid? TargetUserId { get; set; }
    public User? TargetUser { get; set; }

    public AssignmentMode AssignmentMode { get; set; } = AssignmentMode.Automatic;
    public AssignmentAlgorithm AssignmentAlgorithm { get; set; } = AssignmentAlgorithm.RoundRobin;

    protected WorkflowLevel() { }

    public WorkflowLevel(
        Guid workflowId, 
        int levelNumber, 
        string name, 
        string? description, 
        AssignmentTargetType targetType,
        Guid? targetRoleId, 
        Guid? targetUserId,
        AssignmentMode assignmentMode = AssignmentMode.Automatic,
        AssignmentAlgorithm assignmentAlgorithm = AssignmentAlgorithm.RoundRobin)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        LevelNumber = levelNumber;
        Name = name;
        Description = description;
        TargetType = targetType;
        TargetRoleId = targetRoleId;
        TargetUserId = targetUserId;
        AssignmentMode = assignmentMode;
        AssignmentAlgorithm = assignmentAlgorithm;
        CreatedAt = DateTime.UtcNow;
    }
}
