using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseFinding : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string Description { get; set; } = null!;
}
