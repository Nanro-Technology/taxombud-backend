using TaxOmbud.Application.Webhooks.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IWebhooksService
{
    Task<Response<CreatedWebhookResponse>> CreateWebhookAsync(CreateWebhookCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteWebhookAsync(DeleteWebhookCommand request, CancellationToken cancellationToken = default);
    Task<Response<RotateSecretResponseDto>> RotateWebhookSecretAsync(RotateWebhookSecretCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateWebhookAsync(UpdateWebhookCommand request, CancellationToken cancellationToken = default);
    Task<Response<WebhookDetailDto>> GetWebhookByIdAsync(GetWebhookByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<WebhookDto>>> GetWebhooksAsync(GetWebhooksQuery request, CancellationToken cancellationToken = default);
}
