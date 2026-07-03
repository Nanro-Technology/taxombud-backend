namespace TaxOmbud.Application.Webhooks.DTOs;

public record GetWebhookByIdQuery(Guid Id) ;

public record WebhookDetailDto(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
