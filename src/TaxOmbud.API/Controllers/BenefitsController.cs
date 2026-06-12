using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/benefits")]
public class BenefitsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public BenefitsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("types")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult GetBenefitTypes(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("types")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateBenefitType(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpGet("assignments")]
    public IActionResult GetEmployeeBenefits([FromQuery] Guid? employeeId, CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost("assignments")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult AssignBenefit(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
