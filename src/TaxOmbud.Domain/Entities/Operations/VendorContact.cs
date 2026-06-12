using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Operations;

public class VendorContact : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Designation { get; set; }
    public string? Scope { get; set; } // Open, All
    public string? ScopeTarget { get; set; }
    public string? Notes { get; set; }
}
