using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/payroll")]
public class PayrollController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public PayrollController(IMediator mediator) => _mediator = mediator;

    [HttpGet("profiles")]
    public IActionResult GetProfiles(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("runs")]
    public IActionResult GetRuns(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("execute")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult ExecuteRun(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
    
    [HttpGet("deductions")]
    public IActionResult GetDeductions(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
    
    [HttpGet("remittances")]
    public IActionResult GetRemittances(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
