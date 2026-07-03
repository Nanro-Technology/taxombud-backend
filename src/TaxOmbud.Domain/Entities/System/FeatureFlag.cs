using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class FeatureFlag : BaseEntity
{
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
}
