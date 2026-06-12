using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/exit-management")]
public class ExitManagementController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public ExitManagementController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public IActionResult GetAllExits(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult RequestExit(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
    
    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult ApproveExit(Guid id, CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
