using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayoutProvider : BaseEntity
{
    public string Name { get; set; } = null!;
    public string ProviderCode { get; set; } = null!;
    public string Type { get; set; } = "Bank";
    
    public string Status { get; set; } = "Active";
}
