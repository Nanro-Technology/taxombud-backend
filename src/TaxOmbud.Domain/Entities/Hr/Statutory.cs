using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class StatutoryDeduction : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Country { get; set; } = "NG";
    
    public decimal EmployeePercentage { get; set; }
    public decimal EmployerPercentage { get; set; }
    
    public string Status { get; set; } = "Active";
}

public class StatutoryRule : BaseEntity
{
    public Guid DeductionId { get; set; }
    public StatutoryDeduction Deduction { get; set; } = null!;
    
    public string AppliesTo { get; set; } = "All";
    public string Basis { get; set; } = "Gross";
    public decimal RateOrAmount { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
