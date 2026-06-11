using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using System;

namespace TaxOmbud.Application.Features.Roles.Queries.GetRoles;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetRolesQuery() : IRequest<Result<IEnumerable<RoleDto>>>;

public record RoleDto(Guid Id, string Name, string Code, string Scope, string? Description);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(r.Id, r.Name, r.Code, r.Scope, r.Description))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<RoleDto>>.Success(roles);
    }
}
