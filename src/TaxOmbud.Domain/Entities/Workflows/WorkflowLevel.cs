using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

public class WorkflowLevel : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    public int LevelNumber { get; set; } // 1-based order within the workflow

    public string Name { get; set; } = null!;  // e.g. "Stage 1 – Intake"
    public string? Description { get; set; }

    /// <summary>
    /// Semantic role of this level. Drives Case.CurrentStage and CaseStatus
    /// independently of LevelNumber, enabling N-level workflows.
    /// Every workflow must contain at least one level where LevelRole = AdmissibilityGate.
    /// </summary>
    public LevelRole LevelRole { get; set; } = LevelRole.Custom;

    public int? SlaHours { get; set; }
    public int? EscalationHours { get; set; }

    public bool IsMandatory { get; set; } = true;
    public bool RequireComment { get; set; } = false;
    public bool RequireAttachment { get; set; } = false;

    public AssignmentMode AssignmentMode { get; set; } = AssignmentMode.Automatic;
    public AssignmentAlgorithm AssignmentAlgorithm { get; set; } = AssignmentAlgorithm.RoundRobin;

    /// <summary>
    /// Multi-target assignment targets for this level.
    /// May contain any combination of Role, Department, and User targets.
    /// Routing engine intersects Department + Role memberships when resolving assignees.
    /// Specific User targets bypass the role/department filter.
    /// </summary>
    public ICollection<WorkflowLevelTarget> Targets { get; set; } = new List<WorkflowLevelTarget>();

    protected WorkflowLevel() { }

    public WorkflowLevel(
        Guid workflowId,
        int levelNumber,
        string name,
        string? description,
        LevelRole levelRole = LevelRole.Custom,
        AssignmentMode assignmentMode = AssignmentMode.Automatic,
        AssignmentAlgorithm assignmentAlgorithm = AssignmentAlgorithm.RoundRobin)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        LevelNumber = levelNumber;
        Name = name;
        Description = description;
        LevelRole = levelRole;
        AssignmentMode = assignmentMode;
        AssignmentAlgorithm = assignmentAlgorithm;
        CreatedAt = DateTime.UtcNow;
    }
}
