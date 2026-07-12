using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class EmployeeWallet : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal BalanceNgn { get; set; }
    public int LedgerVersion { get; set; }
    public string Status { get; set; } = "active";

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
