namespace TaxOmbud.Application.Webhooks.DTOs;

public record GetWebhooksQuery() ;

public record WebhookDto(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
