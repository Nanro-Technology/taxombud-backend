using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/tickets")]
public class TicketsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public TicketsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetTickets(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult CreateTicket(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
