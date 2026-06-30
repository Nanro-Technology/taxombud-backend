using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.System.DTOs;

namespace TaxOmbud.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly ISystemService _systemService;

    public AnnouncementsController(ISystemService systemService)
    {
        _systemService = systemService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll([FromQuery] bool unreadOnly = false)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetById(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementCommand command, CancellationToken ct)
    {
        var result = await _systemService.CreateAnnouncementAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult MarkAsRead(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Delete(Guid id)
    {
        return Ok(new { Message = "Not implemented yet" });
    }
}
