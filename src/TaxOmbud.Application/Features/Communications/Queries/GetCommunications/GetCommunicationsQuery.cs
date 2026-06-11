using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Communications.Queries.GetCommunications;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetCommunicationsQuery(
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? Channel,
    string? Direction,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<CommunicationListDto>>>;

public record CommunicationListDto(
    Guid Id,
    string Channel,
    string Direction,
    string Subject,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsSent,
    DateTimeOffset? SentAt,
    string? ErrorMessage,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetCommunicationsQueryHandler : IRequestHandler<GetCommunicationsQuery, Result<PagedResult<CommunicationListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CommunicationListDto>>> Handle(GetCommunicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CommunicationLogs.AsNoTracking().AsQueryable();

        if (request.RelatedEntityId.HasValue)
            query = query.Where(c => c.RelatedEntityId == request.RelatedEntityId.Value);

        if (!string.IsNullOrWhiteSpace(request.RelatedEntityType))
            query = query.Where(c => c.RelatedEntityType == request.RelatedEntityType);

        if (!string.IsNullOrWhiteSpace(request.Channel))
            query = query.Where(c => c.Channel.ToLower() == request.Channel.ToLower());

        if (!string.IsNullOrWhiteSpace(request.Direction) && Enum.TryParse<CommunicationDirection>(request.Direction, true, out var dir))
            query = query.Where(c => c.Direction == dir);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CommunicationListDto(
                c.Id,
                c.Channel,
                c.Direction.ToString(),
                c.Subject,
                c.Recipient,
                c.RecipientName,
                c.RelatedEntityId,
                c.RelatedEntityType,
                c.IsSent,
                c.SentAt,
                c.ErrorMessage,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<CommunicationListDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<CommunicationListDto>>.Success(pagedResult);
    }
}
