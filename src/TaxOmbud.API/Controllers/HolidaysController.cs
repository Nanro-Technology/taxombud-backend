using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/holidays")]
public class HolidaysController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public HolidaysController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetHolidays(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateHoliday(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
