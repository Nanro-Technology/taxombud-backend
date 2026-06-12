using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using TaxOmbud.Application.Features.System.Commands.CreateAnnouncement;

namespace TaxOmbud.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnnouncementsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetAll([FromQuery] bool unreadOnly = false)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create(CreateAnnouncementCommand command)
    {
        return Ok(_mediator.Send(command).Result);
    }

    [HttpPut("{id:guid}/read")]
    public IActionResult MarkAsRead(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }
}

