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
[Route("api/v1/visitors")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class VisitorsController : ControllerBase
{
    private readonly IOperationsService _operationsService;

    public VisitorsController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVisitors([FromQuery] GetVisitorsQueries query, CancellationToken ct)
    {
        var result = await _operationsService.GetVisitorsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVisitor([FromBody] CreateVisitorCommands command, CancellationToken ct)
    {
        var result = await _operationsService.CreateVisitorAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVisitorStatus(Guid id, [FromBody] UpdateVisitorStatusCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _operationsService.UpdateVisitorStatusAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVisitor(Guid id, CancellationToken ct)
    {
        var result = await _operationsService.DeleteVisitorAsync(new DeleteVisitorCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
