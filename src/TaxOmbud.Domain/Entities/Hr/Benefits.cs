using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class BenefitType : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Category { get; set; } = null!; // Medical, Pension, Life, Housing, Transport, Education, Custom
    
    public bool AffectsPayroll { get; set; } = false;
    public bool IsTaxable { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
}

public class EmployeeBenefit : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    
    public Guid BenefitTypeId { get; set; }
    public BenefitType BenefitType { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public decimal? AmountOrValue { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
}
