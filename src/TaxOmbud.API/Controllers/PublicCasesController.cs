using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicCasesController : ControllerBase
{
    private readonly ICasesService _casesService;

    public PublicCasesController(ICasesService casesService)
    {
        _casesService = casesService;
    }

    [HttpPost("case")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitCase([FromBody] SubmitPublicCaseCommand command, CancellationToken ct)
    {
        var result = await _casesService.SubmitPublicCaseAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("track_complaints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrackComplaint([FromForm] string trackingNumber, CancellationToken ct)
    {
        var result = await _casesService.TrackComplaintAsync(new TrackComplaintQuery(trackingNumber), ct);
        return StatusCode(result.StatusCode, result);
    }
}
