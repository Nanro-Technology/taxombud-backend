using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class LoanRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Purpose { get; set; } = null!;
    public string? DisburseTo { get; set; } // Bank Account, Wallet, etc.
    public string? PayoutReference { get; set; }
    public string? ActionNote { get; set; }

    public string Status { get; set; } = "pending"; // pending, approved, rejected, disbursed, paid_off
    public bool IsSalaryAdvance { get; set; } = false;
    
    public string? ApprovalChain { get; set; } // JSON list of approvers and outcomes
    public string? RepaymentSchedule { get; set; } // JSON list of payment dates and amounts
}
