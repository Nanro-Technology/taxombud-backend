using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Roles.Queries.GetRoleById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleDetailDto>>;

public record RoleDetailDto(
    Guid Id,
    string Name,
    string Code,
    string Scope,
    string? Description,
    IEnumerable<PermissionDto> Permissions
);

public record PermissionDto(string Code, string Action, string Entity, string? Description);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            return Result<RoleDetailDto>.NotFound("Role not found.");

        var dto = new RoleDetailDto(
            role.Id,
            role.Name,
            role.Code,
            role.Scope,
            role.Description,
            role.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission!.Code,
                rp.Permission.Action,
                rp.Permission.Entity,
                rp.Permission.Description
            ))
        );

        return Result<RoleDetailDto>.Success(dto);
    }
}
