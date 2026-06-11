using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Identity;

public class MfaToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string SecretKey { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string BackupCodesHash { get; set; } = null!; // BCrypt hashed backup codes list
}
