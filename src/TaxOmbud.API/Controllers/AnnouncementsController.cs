using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.System.Commands.CreateAnnouncement;

namespace TaxOmbud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnnouncementsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
