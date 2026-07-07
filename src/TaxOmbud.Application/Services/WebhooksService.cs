using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Webhooks.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Services;

public class WebhooksService : IWebhooksService
{
    private readonly IGenericRepository<WebhookSubscription> _webhookRepo;

    public WebhooksService(IGenericRepository<WebhookSubscription> webhookRepo)
    {
        _webhookRepo = webhookRepo;
    }

    public async Task<Response<CreatedWebhookResponse>> CreateWebhookAsync(CreateWebhookCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreatedWebhookResponse>();
        try
        {
            var webhook = new WebhookSubscription
            {
                Id = Guid.NewGuid(),
                Url = request.Url,
                Secret = request.Secret,
                EventTypes = string.Join(",", request.EventTypes),
                IsActive = true
            };

            await _webhookRepo.AddAsync(webhook);
            await _webhookRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook subscription created successfully.";
            response.Data = new CreatedWebhookResponse(webhook.Id, webhook.Url, webhook.EventTypes, webhook.IsActive);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the webhook subscription.";
        }
        return response;
    }

    public async Task<Response<object?>> DeleteWebhookAsync(DeleteWebhookCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var webhook = await _webhookRepo.FindAsync(w => w.Id == request.Id);

            if (webhook == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Webhook subscription not found.";
                return response;
            }

            await _webhookRepo.RemoveAsync(webhook);
            await _webhookRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook subscription deleted successfully.";
            response.Data = null;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the webhook subscription.";
        }
        return response;
    }

    public async Task<Response<RotateSecretResponseDto>> RotateWebhookSecretAsync(RotateWebhookSecretCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RotateSecretResponseDto>();
        try
        {
            var webhook = await _webhookRepo.FindAsync(w => w.Id == request.Id);

            if (webhook == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Webhook subscription not found.";
                return response;
            }

            webhook.Secret = request.NewSecret;
            webhook.LastModifiedAt = DateTime.UtcNow;
            await _webhookRepo.UpdateAsync(webhook);
            await _webhookRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook secret rotated successfully.";
            response.Data = new RotateSecretResponseDto("Secret rotated successfully.");
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while rotating the webhook secret.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateWebhookAsync(UpdateWebhookCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var webhook = await _webhookRepo.FindAsync(w => w.Id == request.Id);

            if (webhook == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Webhook subscription not found.";
                return response;
            }

            webhook.Url = request.Url;
            webhook.EventTypes = string.Join(",", request.EventTypes);
            webhook.IsActive = request.IsActive;
            webhook.LastModifiedAt = DateTime.UtcNow;

            await _webhookRepo.UpdateAsync(webhook);
            await _webhookRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook subscription updated successfully.";
            response.Data = null;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the webhook subscription.";
        }
        return response;
    }

    public async Task<Response<WebhookDetailDto>> GetWebhookByIdAsync(GetWebhookByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<WebhookDetailDto>();
        try
        {
            var webhook = await _webhookRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (webhook == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Webhook subscription not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook subscription retrieved successfully.";
            response.Data = new WebhookDetailDto(
                webhook.Id,
                webhook.Url,
                webhook.EventTypes,
                webhook.IsActive,
                webhook.CreatedAt,
                webhook.LastModifiedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the webhook subscription.";
        }
        return response;
    }

    public async Task<Response<IEnumerable<WebhookDto>>> GetWebhooksAsync(GetWebhooksQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<WebhookDto>>();
        try
        {
            var webhooks = await _webhookRepo.Query()
                .AsNoTracking()
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WebhookDto(
                    w.Id,
                    w.Url,
                    w.EventTypes,
                    w.IsActive,
                    w.CreatedAt,
                    w.LastModifiedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Webhook subscriptions retrieved successfully.";
            response.Data = webhooks;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving webhook subscriptions.";
        }
        return response;
    }
}
