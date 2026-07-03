namespace TaxOmbud.Application.Webhooks.DTOs;

public record UpdateWebhookCommand(
    Guid Id,
    string Url,
    string[] EventTypes,
    bool IsActive
) ;

public record UpdateWebhookRequest(string Url, string[] EventTypes, bool IsActive);
