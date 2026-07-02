using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Roles.DTOs;

public record GetRoleByIdQuery(Guid Id);

public record RoleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    bool IsActive,
    IEnumerable<PermissionDto> Permissions
);

/// <summary>Permission DTO using Module × Action enum-derived strings (e.g. "Complaints", "View").</summary>
public record PermissionDto(Guid Id, string Module, string Action);