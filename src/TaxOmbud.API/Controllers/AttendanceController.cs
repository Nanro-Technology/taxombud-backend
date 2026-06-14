using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/attendance")]
public class AttendanceController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public IActionResult GetAttendanceLogs(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("clock-in")]
    public IActionResult ClockIn(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("clock-out")]
    public IActionResult ClockOut(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
