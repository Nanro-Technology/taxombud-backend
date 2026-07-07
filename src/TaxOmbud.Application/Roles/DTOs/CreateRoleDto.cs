namespace TaxOmbud.Application.Roles.DTOs;

/// <summary>
/// Creates a new staff role with a mandatory set of permission IDs.
/// A role cannot be created without at least one permission — an empty role has no meaning.
/// Only applicable to StaffUser type users.
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description,
    IReadOnlyList<Guid> PermissionIds
);

public record CreateRoleResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<PermissionDto> Permissions
);
