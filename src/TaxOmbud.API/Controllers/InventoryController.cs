using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Operations.Queries.GetInventoryItems;
using TaxOmbud.Application.Features.Operations.Queries.GetVendors;
using TaxOmbud.Application.Features.Operations.Commands.AddInventoryItem;
using TaxOmbud.Application.Features.Operations.Commands.AddVendor;

namespace TaxOmbud.Api.Controllers;

public class InventoryController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("items")]
    public async Task<IActionResult> GetInventoryItems([FromQuery] GetInventoryItemsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors([FromQuery] GetVendorsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost("items")]
    public async Task<IActionResult> AddInventoryItem([FromBody] AddInventoryItemCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("vendors")]
    public async Task<IActionResult> AddVendor([FromBody] AddVendorCommands command) => ToActionResult(await _mediator.Send(command));
}