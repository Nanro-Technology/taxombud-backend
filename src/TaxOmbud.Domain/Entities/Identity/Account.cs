using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Identity;

public class Account : BaseAuditableEntity
{
    public string Name { get; set; } = null!; // Zone name, e.g. "South West"
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Country { get; set; } = "Nigeria";
    public string Status { get; set; } = "active";
    public int HealthScore { get; set; } = 100;
    public string? Description { get; set; }
    public bool IsWorkflowLane { get; set; } = true;
}
