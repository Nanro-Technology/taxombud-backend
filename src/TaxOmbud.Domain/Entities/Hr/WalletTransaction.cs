using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public EmployeeWallet Wallet { get; set; } = null!;

    public string Type { get; set; } = "credit"; // credit, debit
    public decimal Amount { get; set; }
    public string Reference { get; set; } = null!; // transaction reference

    public string Status { get; set; } = "pending"; // pending, approved, paid, rejected
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? BankDetail { get; set; }
    public string? ProviderRef { get; set; }
    public int AttemptNumber { get; set; } = 1;
}
