using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/shifts")]
public class ShiftsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public ShiftsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetShifts(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateShift(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("assignments")]
    public IActionResult GetAssignments(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("assignments")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult AssignShift(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
