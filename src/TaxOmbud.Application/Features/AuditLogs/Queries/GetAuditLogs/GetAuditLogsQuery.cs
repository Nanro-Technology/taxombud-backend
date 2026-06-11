using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.AuditLogs.Queries.GetAuditLogs;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAuditLogsQuery(
    string? EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AuditLogDto>>>;

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid? EntityId,
    string Action,
    Guid? UserId,
    Guid? ImpersonatorUserId,
    string? OldValues,
    string? NewValues,
    string? IPAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(l => l.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(l => l.EntityId == request.EntityId.Value);

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(l => l.Action == request.Action);

        if (request.From.HasValue)
            query = query.Where(l => l.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(l => l.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new AuditLogDto(
                l.Id,
                l.EntityType,
                l.EntityId,
                l.Action,
                l.UserId,
                l.ImpersonatorUserId,
                l.OldValues,
                l.NewValues,
                l.IPAddress,
                l.UserAgent,
                l.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<AuditLogDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<AuditLogDto>>.Success(pagedResult);
    }
}
