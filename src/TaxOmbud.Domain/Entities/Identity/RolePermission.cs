using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// Join table linking a Role to a Permission (both by Guid FK).
/// </summary>
public class RolePermission : BaseAuditableEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
