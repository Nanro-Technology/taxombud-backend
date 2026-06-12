using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/performance")]
public class PerformanceController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public PerformanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("cycles")]
    public IActionResult GetCycles(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("cycles")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateCycle(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("goals")]
    public IActionResult GetGoals([FromQuery] Guid? employeeId, CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("goals")]
    public IActionResult CreateGoal(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("reviews")]
    public IActionResult GetReviews([FromQuery] Guid? employeeId, CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("reviews")]
    public IActionResult CreateReview(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
