using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// A system-managed role that aggregates permissions.
/// Users are assigned exactly ONE role via a FK on the User entity (Estate Management pattern).
/// System roles (IsSystemRole = true) cannot be deleted — only deactivated.
/// </summary>
public class Role : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>
    /// True for seeded built-in roles (e.g. Super Admin).
    /// These cannot be deleted through the UI — only deactivated.
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>Whether this role can be assigned to users.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
