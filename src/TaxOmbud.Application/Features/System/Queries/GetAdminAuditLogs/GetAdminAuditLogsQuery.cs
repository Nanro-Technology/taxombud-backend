using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.System.Queries.GetAdminAuditLogs;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAdminAuditLogsQuery(
    string? EntityName,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AuditLog>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAdminAuditLogsQueryHandler : IRequestHandler<GetAdminAuditLogsQuery, Result<PagedResult<AuditLog>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AuditLog>>> Handle(GetAdminAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(l => l.EntityType == request.EntityName);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<AuditLog>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<AuditLog>>.Success(pagedResult);
    }
}
