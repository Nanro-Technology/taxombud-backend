using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayrollEntry : BaseEntity
{
    public Guid RunId { get; set; }
    public PayrollRun Run { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Basic { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }

    public decimal Paye { get; set; }
    public decimal Pension { get; set; }
    public decimal Nhf { get; set; }
    public decimal OtherStatutory { get; set; }

    public decimal Gross { get; set; }
    public decimal Net { get; set; }

    public string PaymentStatus { get; set; } = "pending"; // pending, paid, failed
}
