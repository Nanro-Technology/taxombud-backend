using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Communications.Commands.LogCommunication;
using TaxOmbud.Application.Features.Communications.Queries.GetCommunicationById;
using TaxOmbud.Application.Features.Communications.Queries.GetCommunications;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Track all communications (email, SMS, in-app, letter) sent to or received from taxpayers.
/// </summary>
[Authorize(Policy = "OfficerOrAbove")]
[Route("api/v1/communications")]
public class CommunicationsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CommunicationsController(IMediator mediator)
    {
        _mediator = mediator;
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
        var result = await _mediator.Send(new GetCommunicationsQuery(
            relatedEntityId, relatedEntityType, channel, direction, page, pageSize
        ), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a specific communication log entry by ID including full body.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunicationById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCommunicationByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Manually log an outbound communication sent outside the system.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogCommunication([FromBody] LogCommunicationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LogCommunicationCommand(
            request.Channel,
            request.Subject,
            request.Body,
            request.Recipient,
            request.RecipientName,
            request.RelatedEntityId,
            request.RelatedEntityType
        ), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetCommunicationById), new { id = result.Value!.Id }, result.Value);
    }
}

public record LogCommunicationRequest(
    string Channel,
    string Subject,
    string Body,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType
);
