using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Operations.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class InventoryController : ControllerBase
{
    private readonly IOperationsService _operationsService;

    public InventoryController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItems([FromQuery] GetInventoryItemsQueries query, CancellationToken ct)
    {
        var result = await _operationsService.GetInventoryItemsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInventoryItem([FromBody] AddInventoryItemCommands command, CancellationToken ct)
    {
        var result = await _operationsService.AddInventoryItemAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _operationsService.UpdateInventoryItemAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventoryItemStatus(Guid id, [FromBody] UpdateInventoryItemStatusCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _operationsService.UpdateInventoryItemStatusAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInventoryItem(Guid id, CancellationToken ct)
    {
        var result = await _operationsService.DeleteInventoryItemAsync(new DeleteInventoryItemCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
