using System;

namespace TaxOmbud.Domain.Entities.Identity;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string? ScopeQualifier { get; set; } // e.g. lane_id or zone_id
}
