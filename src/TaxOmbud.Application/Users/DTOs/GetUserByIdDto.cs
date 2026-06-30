using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record GetUserByIdQuery(Guid Id) ;

public record UserDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? AltPhone,
    string? JobTitle,
    string? EmploymentType,
    DepartmentDetailDto? Department,
    string Status,
    bool CanSignIn,
    IEnumerable<RoleDetailDto> Roles,
    IEnumerable<PermissionOverrideDetailDto> PermissionOverrides
);

public record DepartmentDetailDto(Guid Id, string Name);
public record RoleDetailDto(Guid Id, string Name, string Code);
public record PermissionOverrideDetailDto(string PermissionCode, string Mode);