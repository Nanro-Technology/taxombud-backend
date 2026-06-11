using System;

namespace TaxOmbud.Domain.Entities.Identity;

public class UserPermissionOverride
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string PermissionCode { get; set; } = null!;
    public Permission Permission { get; set; } = null!;

    public string Mode { get; set; } = "grant"; // grant or deny
}
