using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Workflows;

/// <summary>
/// Admin-managed library of named workflow stage templates.
/// System stages (IsSystemStage = true) are seeded on startup and cannot be deleted.
/// Administrators can add custom stages (e.g., "Legal Review", "Appeals Triage").
/// When creating a workflow, users can "Pick from Library" to pre-populate a level
/// with the library item's name, description, SLA defaults, and LevelRole.
/// </summary>
public class WorkflowStageLibraryItem : BaseEntity
{
    public string Name { get; set; } = null!;          // e.g. "Stage 4 – Admissibility Assessment"
    public string? Description { get; set; }
    public LevelRole LevelRole { get; set; } = LevelRole.Custom;
    public int? DefaultSlaHours { get; set; }
    public int? DefaultEscalationHours { get; set; }
    public AssignmentAlgorithm DefaultAlgorithm { get; set; } = AssignmentAlgorithm.RoundRobin;

    /// <summary>When true, this stage cannot be deleted via the API (system-owned).</summary>
    public bool IsSystemStage { get; set; } = false;

    /// <summary>Suggested display order in the Stage Library panel.</summary>
    public int OrderHint { get; set; } = 0;

    protected WorkflowStageLibraryItem() { }

    public WorkflowStageLibraryItem(
        string name,
        string? description,
        LevelRole levelRole,
        int? defaultSlaHours,
        int? defaultEscalationHours,
        AssignmentAlgorithm defaultAlgorithm = AssignmentAlgorithm.RoundRobin,
        bool isSystemStage = false,
        int orderHint = 0)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        LevelRole = levelRole;
        DefaultSlaHours = defaultSlaHours;
        DefaultEscalationHours = defaultEscalationHours;
        DefaultAlgorithm = defaultAlgorithm;
        IsSystemStage = isSystemStage;
        OrderHint = orderHint;
        CreatedAt = DateTime.UtcNow;
    }
}
