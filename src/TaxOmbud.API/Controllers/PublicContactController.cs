using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Contact.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public PublicContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost("contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitContact([FromForm] SubmitContactFormCommand command, CancellationToken ct)
    {
        var result = await _contactService.SubmitContactFormAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
