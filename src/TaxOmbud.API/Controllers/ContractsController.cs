using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/contracts")]
public class ContractsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public ContractsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetContracts(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult CreateContract(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
    
    [HttpPost("{id}/review")]
    public IActionResult InitiateReview(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
