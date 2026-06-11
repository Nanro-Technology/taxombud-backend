using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Webhooks.Queries.GetWebhookById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetWebhookByIdQuery(Guid Id) : IRequest<Result<WebhookDetailDto>>;

public record WebhookDetailDto(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetWebhookByIdQueryHandler : IRequestHandler<GetWebhookByIdQuery, Result<WebhookDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWebhookByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WebhookDetailDto>> Handle(GetWebhookByIdQuery request, CancellationToken cancellationToken)
    {
        var webhook = await _context.WebhookSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (webhook == null)
            return Result<WebhookDetailDto>.NotFound("Webhook subscription not found.");

        var dto = new WebhookDetailDto(
            webhook.Id,
            webhook.Url,
            webhook.EventTypes,
            webhook.IsActive,
            webhook.CreatedAt,
            webhook.UpdatedAt
        );

        return Result<WebhookDetailDto>.Success(dto);
    }
}
