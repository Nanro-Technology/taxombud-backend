using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Webhooks.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage outgoing webhook subscriptions for real-time event delivery to third-party systems.
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhooksService _webhooksService;

    public WebhooksController(IWebhooksService webhooksService)
    {
        _webhooksService = webhooksService;
    }

    /// <summary>List all webhook subscriptions.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(CancellationToken ct)
    {
        var result = await _webhooksService.GetWebhooksAsync(new GetWebhooksQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single webhook subscription by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetWebhookById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWebhookById(Guid id, CancellationToken ct)
    {
        var result = await _webhooksService.GetWebhookByIdAsync(new GetWebhookByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new webhook subscription. Secret is HMAC-SHA256 signing key.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookRequest request, CancellationToken ct)
    {
        var result = await _webhooksService.CreateWebhookAsync(new CreateWebhookCommand(request.Url, request.Secret, request.EventTypes), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetWebhookById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update webhook URL, events, or toggle active state.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWebhook(Guid id, [FromBody] UpdateWebhookRequest request, CancellationToken ct)
    {
        var result = await _webhooksService.UpdateWebhookAsync(new UpdateWebhookCommand(id, request.Url, request.EventTypes, request.IsActive), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Rotate the HMAC signing secret for a webhook.</summary>
    [HttpPost("{id:guid}/rotate-secret")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RotateSecret(Guid id, [FromBody] RotateSecretRequest request, CancellationToken ct)
    {
        var result = await _webhooksService.RotateWebhookSecretAsync(new RotateWebhookSecretCommand(id, request.NewSecret), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a webhook subscription.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(Guid id, CancellationToken ct)
    {
        var result = await _webhooksService.DeleteWebhookAsync(new DeleteWebhookCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
