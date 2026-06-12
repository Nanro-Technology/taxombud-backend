using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class Holiday : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public string Type { get; set; } = "Public"; // Public, Company
    public string Mode { get; set; } = "One-off"; // Recurring, One-off
    
    public string AppliesTo { get; set; } = "All"; // All, Department
    public Guid? DepartmentId { get; set; }
}
