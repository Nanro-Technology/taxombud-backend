using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class EwaRequest : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending"; // pending, approved, disbursed, rejected
    public DateTimeOffset? DisbursedAt { get; set; }
    public Guid? RecoveredInPeriodId { get; set; }
    public PayrollPeriod? RecoveredInPeriod { get; set; }
}
