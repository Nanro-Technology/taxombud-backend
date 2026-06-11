using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Webhooks.Queries.GetWebhooks;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetWebhooksQuery() : IRequest<Result<IEnumerable<WebhookDto>>>;

public record WebhookDto(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetWebhooksQueryHandler : IRequestHandler<GetWebhooksQuery, Result<IEnumerable<WebhookDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetWebhooksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<WebhookDto>>> Handle(GetWebhooksQuery request, CancellationToken cancellationToken)
    {
        var webhooks = await _context.WebhookSubscriptions
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WebhookDto(
                w.Id,
                w.Url,
                w.EventTypes,
                w.IsActive,
                w.CreatedAt,
                w.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<WebhookDto>>.Success(webhooks);
    }
}
