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
[Route("api/v1/vendor-contacts")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class VendorContactsController : ControllerBase
{
    private readonly IOperationsService _operationsService;

    public VendorContactsController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendors(CancellationToken ct)
    {
        var result = await _operationsService.GetVendorsAsync(new GetVendorsQueries(), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVendorById(Guid id, CancellationToken ct)
    {
        var result = await _operationsService.GetVendorByIdAsync(new GetVendorByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddVendor([FromBody] AddVendorCommands command, CancellationToken ct)
    {
        var result = await _operationsService.AddVendorAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] UpdateVendorCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in route does not match ID in payload.");
        }
        var result = await _operationsService.UpdateVendorAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVendor(Guid id, CancellationToken ct)
    {
        var result = await _operationsService.DeleteVendorAsync(new DeleteVendorCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
