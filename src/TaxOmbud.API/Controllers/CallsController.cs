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

/// <summary>Manage CRM Calls — list, create, update, delete.</summary>
[ApiController]
[Route("api/v1/calls")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class CallsController : ControllerBase
{
    private readonly ICrmService _crmService;

    public CallsController(ICrmService crmService)
    {
        _crmService = crmService;
    }

    /// <summary>List all logged calls.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Response<List<CallDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalls(CancellationToken ct)
    {
        var items = await _crmService.GetCallsAsync(new GetCallsQuery(), ct);
        return Ok(new Response<List<CallDto>>
        {
            StatusCode = 200,
            Message = "Calls retrieved successfully.",
            Data = items
        });
    }

    /// <summary>Get a single call by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetCallById")]
    [ProducesResponseType(typeof(Response<CallDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCallById(Guid id, CancellationToken ct)
    {
        try
        {
            var item = await _crmService.GetCallByIdAsync(new GetCallByIdQuery(id), ct);
            return Ok(new Response<CallDto> { StatusCode = 200, Message = "Call retrieved.", Data = item });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Log a new call.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCall([FromBody] CreateCallRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(new Response<object> { StatusCode = 400, Message = "Subject is required." });

        var id = await _crmService.CreateCallAsync(
            new CreateCallCommand(
                request.Subject,
                request.CallerType ?? "Personal",
                request.CallerMethod ?? "Phone",
                request.CallerIdentifier ?? string.Empty,
                request.CalleeMethod ?? "Phone",
                request.CalleeIdentifier ?? string.Empty,
                request.Direction ?? "Inbound",
                request.Status ?? "Answered",
                request.Phone,
                request.Notes,
                request.LinkedToId,
                request.AgentId,
                request.StartAt ?? DateTimeOffset.UtcNow,
                request.EndAt
            ), ct);

        return CreatedAtRoute("GetCallById", new { id },
            new Response<Guid> { StatusCode = 201, Message = "Call logged successfully.", Data = id });
    }

    /// <summary>Update an existing call.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCall(Guid id, [FromBody] CreateCallRequest request, CancellationToken ct)
    {
        try
        {
            await _crmService.UpdateCallAsync(
                new UpdateCallCommand(
                    id,
                    request.Subject ?? string.Empty,
                    request.CallerType ?? "Personal",
                    request.CallerMethod ?? "Phone",
                    request.CallerIdentifier ?? string.Empty,
                    request.CalleeMethod ?? "Phone",
                    request.CalleeIdentifier ?? string.Empty,
                    request.Direction ?? "Inbound",
                    request.Status ?? "Answered",
                    request.Phone,
                    request.Notes,
                    request.LinkedToId,
                    request.AgentId,
                    request.StartAt ?? DateTimeOffset.UtcNow,
                    request.EndAt
                ), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Call updated successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Delete a call.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCall(Guid id, CancellationToken ct)
    {
        try
        {
            await _crmService.DeleteCallAsync(new DeleteCallCommand(id), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Call deleted successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }
}

public record CreateCallRequest(
    string? Subject,
    string? CallerType,
    string? CallerMethod,
    string? CallerIdentifier,
    string? CalleeMethod,
    string? CalleeIdentifier,
    string? Direction,
    string? Status,
    string? Phone,
    string? Notes,
    Guid? LinkedToId,
    Guid? AgentId,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt
);
