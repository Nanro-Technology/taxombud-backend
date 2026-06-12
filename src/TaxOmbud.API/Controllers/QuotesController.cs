using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/quotes")]
public class QuotesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public QuotesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetQuotes(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult CreateQuote(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
