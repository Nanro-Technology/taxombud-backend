using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record GetRoleByIdQuery(Guid Id) ;

public record RoleDetailDto(
    Guid Id,
    string Name,
    string Code,
    string Scope,
    string? Description,
    IEnumerable<PermissionDto> Permissions
);

public record PermissionDto(string Code, string Action, string Entity, string? Description);