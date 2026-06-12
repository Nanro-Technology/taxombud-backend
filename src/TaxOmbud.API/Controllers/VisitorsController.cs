using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/visitors")]
public class VisitorsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public VisitorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetVisitors(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult CreateVisitor(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
