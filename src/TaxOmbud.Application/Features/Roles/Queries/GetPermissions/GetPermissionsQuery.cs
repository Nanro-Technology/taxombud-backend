using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Roles.Queries.GetPermissions;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetPermissionsQuery() : IRequest<Result<IEnumerable<PermissionDetailDto>>>;

public record PermissionDetailDto(string Code, string Action, string Entity, string? Description);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<IEnumerable<PermissionDetailDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<PermissionDetailDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Entity)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionDetailDto(p.Code, p.Action, p.Entity, p.Description))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<PermissionDetailDto>>.Success(permissions);
    }
}
