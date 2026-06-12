using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class SalaryProfile : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Basic { get; set; }
    
    public string? Allowances { get; set; } // JSON list of allowance items (Housing, Transport)
    public string? Deductions { get; set; } // JSON list of deductions

    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    
    // Extensions for Phase 9
    public Guid? PayGradeId { get; set; }
    public PayGrade? PayGrade { get; set; }
    
    public string Currency { get; set; } = "NGN";
    public string Status { get; set; } = "Active";
}
