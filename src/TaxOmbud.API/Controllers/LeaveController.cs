using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/leave")]
public class LeaveController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public LeaveController(IMediator mediator) => _mediator = mediator;

    [HttpGet("types")]
    public IActionResult GetLeaveTypes(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("types")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateLeaveType(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("balances")]
    public IActionResult GetLeaveBalances(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
