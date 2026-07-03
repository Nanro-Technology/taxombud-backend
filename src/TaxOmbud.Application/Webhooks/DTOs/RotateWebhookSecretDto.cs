namespace TaxOmbud.Application.Webhooks.DTOs;

public record RotateWebhookSecretCommand(Guid Id, string NewSecret) ;

public record RotateSecretResponseDto(string Message);

public record RotateSecretRequest(string NewSecret);
