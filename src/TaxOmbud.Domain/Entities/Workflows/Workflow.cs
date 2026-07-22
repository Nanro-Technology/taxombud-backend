using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Workflows;

public class Workflow : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CaseCategory { get; set; } = "General"; // Default category association
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int CurrentVersion { get; set; } = 1;

    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
    public ICollection<WorkflowLevel> Levels { set; get; } = new List<WorkflowLevel>();

    protected Workflow() { }

    public Workflow(string name, string description, string caseCategory = "General", bool isDefault = false)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CaseCategory = caseCategory;
        IsActive = true;
        IsDefault = isDefault;
        CurrentVersion = 1;
        CreatedAt = DateTime.UtcNow;
    }
}
