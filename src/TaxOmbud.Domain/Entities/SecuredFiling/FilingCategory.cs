using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.SecuredFiling;

public class FilingCategory : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "active"; // active, inactive
}
