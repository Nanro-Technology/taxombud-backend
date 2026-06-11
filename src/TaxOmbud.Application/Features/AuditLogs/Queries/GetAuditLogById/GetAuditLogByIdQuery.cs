using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.AuditLogs.Queries.GetAuditLogById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAuditLogByIdQuery(Guid Id) : IRequest<Result<AuditLogDetailDto>>;

public record AuditLogDetailDto(
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

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AuditLogDetailDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (log == null)
            return Result<AuditLogDetailDto>.NotFound("Audit log entry not found.");

        var dto = new AuditLogDetailDto(
            log.Id,
            log.EntityType,
            log.EntityId,
            log.Action,
            log.UserId,
            log.ImpersonatorUserId,
            log.OldValues,
            log.NewValues,
            log.IPAddress,
            log.UserAgent,
            log.CreatedAt
        );

        return Result<AuditLogDetailDto>.Success(dto);
    }
}
