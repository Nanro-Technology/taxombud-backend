using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Communications.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Track all communications (email, SMS, in-app, letter) sent to or received from taxpayers.
/// </summary>
[ApiController]
[Route("api/v1/communications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "OfficerOrAbove")]
[Produces("application/json")]
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationsService _communicationsService;

    public CommunicationsController(ICommunicationsService communicationsService)
    {
        _communicationsService = communicationsService;
    }

    /// <summary>List communication logs, optionally filtered by entity, channel, or direction.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommunications(
        [FromQuery] Guid? relatedEntityId,
        [FromQuery] string? relatedEntityType,
        [FromQuery] string? channel,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _communicationsService.GetCommunicationsAsync(new GetCommunicationsQuery(
            relatedEntityId, relatedEntityType, channel, direction, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a specific communication log entry by ID including full body.</summary>
    [HttpGet("{id:guid}", Name = "GetCommunicationById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunicationById(Guid id, CancellationToken ct)
    {
        var result = await _communicationsService.GetCommunicationByIdAsync(new GetCommunicationByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of communication templates.</summary>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommunicationTemplates(CancellationToken ct)
    {
        var result = await _communicationsService.GetCommunicationTemplatesAsync(new GetCommunicationTemplatesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Manually log an outbound communication sent outside the system.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogCommunication([FromBody] LogCommunicationRequest request, CancellationToken ct)
    {
        var result = await _communicationsService.LogCommunicationAsync(new LogCommunicationCommand(
            request.Channel, request.Subject, request.Body, request.Recipient,
            request.RecipientName, request.RelatedEntityId, request.RelatedEntityType), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetCommunicationById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Send a communication (email/SMS/in-app).</summary>
    [HttpPost("{id:guid}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendCommunication(Guid id, CancellationToken ct)
    {
        var result = await _communicationsService.SendCommunicationAsync(new SendCommunicationCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Acknowledge receipt or clear error state for a communication.</summary>
    [HttpPatch("{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeCommunication(Guid id, CancellationToken ct)
    {
        var result = await _communicationsService.AcknowledgeCommunicationAsync(new AcknowledgeCommunicationCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Render a communication template with payload data.</summary>
    [HttpPost("templates/{templateId:guid}/render")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenderTemplate(Guid templateId, [FromBody] RenderTemplateRequest request, CancellationToken ct)
    {
        var result = await _communicationsService.RenderCommunicationTemplateAsync(new RenderCommunicationTemplateCommand(templateId, request.Payload), ct);
        return StatusCode(result.StatusCode, result);
    }
}
