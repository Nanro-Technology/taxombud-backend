using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class LoanRequest : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Purpose { get; set; } = null!;

    public string Status { get; set; } = "pending"; // pending, approved, rejected, disbursed, paid_off
    
    public string? ApprovalChain { get; set; } // JSON list of approvers and outcomes
    public string? RepaymentSchedule { get; set; } // JSON list of payment dates and amounts
}
