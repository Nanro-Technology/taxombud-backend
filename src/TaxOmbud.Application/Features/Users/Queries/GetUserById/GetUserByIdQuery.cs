using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Users.Queries.GetUserById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDetailDto>>;

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissionOverrides)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return Result<UserDetailDto>.NotFound("User not found.");

        var dto = new UserDetailDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.Phone,
            user.AltPhone,
            user.JobTitle,
            user.EmploymentType,
            user.Department != null ? new DepartmentDetailDto(user.Department.Id, user.Department.Name) : null,
            user.Status.ToString(),
            user.CanSignIn,
            user.UserRoles.Select(ur => new RoleDetailDto(ur.Role!.Id, ur.Role.Name, ur.Role.Code)),
            user.UserPermissionOverrides.Select(o => new PermissionOverrideDetailDto(o.PermissionCode, o.Mode))
        );

        return Result<UserDetailDto>.Success(dto);
    }
}
