using System;
using System.Text.Json.Serialization;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class StatutoryDeduction : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Country { get; set; } = "NG";
    
    public decimal EmployeePercentage { get; set; }
    public decimal EmployerPercentage { get; set; }
    
    public bool IsEmployee { get; set; } = true;
    public bool IsEmployer { get; set; } = false;
    
    public string Status { get; set; } = "Active";
    public ICollection<StatutoryRule> StatutoryRules { get; set; } = new List<StatutoryRule>();
}

public class StatutoryRule : BaseEntity
{
    public Guid DeductionId { get; set; }
    
    [JsonIgnore]
    public StatutoryDeduction Deduction { get; set; } = null!;
    
    public string AppliesTo { get; set; } = "All";
    public string Basis { get; set; } = "Gross";
    public decimal RateOrAmount { get; set; }
    public string? RateOrAmountStr { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
