using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Crm;

public class Organization : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    
    // Link to primary tax payer or contact
    public Guid? PrimaryTaxPayerId { get; set; }
}
