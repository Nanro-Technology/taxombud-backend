using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Webhooks.Commands.CreateWebhook;
using TaxOmbud.Application.Features.Webhooks.Commands.DeleteWebhook;
using TaxOmbud.Application.Features.Webhooks.Commands.RotateWebhookSecret;
using TaxOmbud.Application.Features.Webhooks.Commands.UpdateWebhook;
using TaxOmbud.Application.Features.Webhooks.Queries.GetWebhookById;
using TaxOmbud.Application.Features.Webhooks.Queries.GetWebhooks;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage outgoing webhook subscriptions for real-time event delivery to third-party systems.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/webhooks")]
public class WebhooksController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List all webhook subscriptions.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWebhooksQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single webhook subscription by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWebhookById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWebhookByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a new webhook subscription. Secret is HMAC-SHA256 signing key.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateWebhookCommand(request.Url, request.Secret, request.EventTypes), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetWebhookById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update webhook URL, events, or toggle active state.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWebhook(Guid id, [FromBody] UpdateWebhookRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateWebhookCommand(id, request.Url, request.EventTypes, request.IsActive), ct);
        return ToActionResult(result);
    }

    /// <summary>Rotate the HMAC signing secret for a webhook.</summary>
    [HttpPost("{id:guid}/rotate-secret")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RotateSecret(Guid id, [FromBody] RotateSecretRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RotateWebhookSecretCommand(id, request.NewSecret), ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a webhook subscription.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteWebhookCommand(id), ct);
        return ToActionResult(result);
    }
}

public record CreateWebhookRequest(string Url, string Secret, string[] EventTypes);
public record UpdateWebhookRequest(string Url, string[] EventTypes, bool IsActive);
public record RotateSecretRequest(string NewSecret);
