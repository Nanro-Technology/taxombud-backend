using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Appeals;

/// <summary>A single ground point raised in an appeal submission.</summary>
public class AppealGroundPoint : BaseEntity
{
    public Guid AppealId { get; set; }
    public Appeal Appeal { get; set; } = null!;

    public int OrderIndex { get; set; }
    public string GroundTitle { get; set; } = null!;
    public string GroundDetail { get; set; } = null!;
    public string? OfficerResponse { get; set; }
}
