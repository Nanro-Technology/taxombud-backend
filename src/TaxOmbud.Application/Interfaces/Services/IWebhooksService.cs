using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Webhooks.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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
