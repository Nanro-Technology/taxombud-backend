using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Crm.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>Manage CRM Interactions — list, create, update, delete.</summary>
[ApiController]
[Route("api/v1/interactions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class InteractionsController : ControllerBase
{
    private readonly ICrmService _crmService;

    public InteractionsController(ICrmService crmService)
    {
        _crmService = crmService;
    }

    /// <summary>List all interactions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Response<List<InteractionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteractions(CancellationToken ct)
    {
        var items = await _crmService.GetInteractionsAsync(new GetInteractionsQuery(), ct);
        return Ok(new Response<List<InteractionDto>>
        {
            StatusCode = 200,
            Message = "Interactions retrieved successfully.",
            Data = items
        });
    }

    /// <summary>Get a single interaction by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetInteractionById")]
    [ProducesResponseType(typeof(Response<InteractionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInteractionById(Guid id, CancellationToken ct)
    {
        try
        {
            var item = await _crmService.GetInteractionByIdAsync(new GetInteractionByIdQuery(id), ct);
            return Ok(new Response<InteractionDto> { StatusCode = 200, Message = "Interaction retrieved.", Data = item });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Log a new interaction.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInteraction([FromBody] CreateInteractionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(new Response<object> { StatusCode = 400, Message = "Subject is required." });
        if (string.IsNullOrWhiteSpace(request.Direction))
            return BadRequest(new Response<object> { StatusCode = 400, Message = "Direction is required." });

        var id = await _crmService.CreateInteractionAsync(
            new CreateInteractionCommand(
                request.Direction,
                request.Subject,
                request.Type ?? "Other",
                request.Channel ?? "Other",
                request.Outcome,
                request.Notes,
                request.RelatedToId,
                request.LoggedById,
                request.OccurredAt ?? DateTimeOffset.UtcNow
            ), ct);

        return CreatedAtRoute("GetInteractionById", new { id },
            new Response<Guid> { StatusCode = 201, Message = "Interaction logged successfully.", Data = id });
    }

    /// <summary>Update an existing interaction.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInteraction(Guid id, [FromBody] CreateInteractionRequest request, CancellationToken ct)
    {
        try
        {
            await _crmService.UpdateInteractionAsync(
                new UpdateInteractionCommand(
                    id,
                    request.Direction ?? "Inbound",
                    request.Subject ?? string.Empty,
                    request.Type ?? "Other",
                    request.Channel ?? "Other",
                    request.Outcome,
                    request.Notes,
                    request.RelatedToId,
                    request.LoggedById,
                    request.OccurredAt ?? DateTimeOffset.UtcNow
                ), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Interaction updated successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Delete an interaction.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInteraction(Guid id, CancellationToken ct)
    {
        try
        {
            await _crmService.DeleteInteractionAsync(new DeleteInteractionCommand(id), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Interaction deleted successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }
}

public record CreateInteractionRequest(
    string? Direction,
    string? Subject,
    string? Type,
    string? Channel,
    string? Outcome,
    string? Notes,
    Guid? RelatedToId,
    Guid? LoggedById,
    DateTimeOffset? OccurredAt
);
