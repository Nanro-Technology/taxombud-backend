using System;

namespace TaxOmbud.Application.Roles.DTOs;

public record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    bool IsActive
);

public record UpdateRoleRequest(
    string Name,
    string? Description,
    bool IsActive
);
