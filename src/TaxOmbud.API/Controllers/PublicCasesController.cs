using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

public class TrackComplaintInput
{
    public string? TrackingNumber { get; set; }
    public string? TrackCode { get; set; }
}

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Route("api/v1/public")]
public class PublicCasesController : ControllerBase
{
    private readonly ICasesService _casesService;

    public PublicCasesController(ICasesService casesService)
    {
        _casesService = casesService;
    }

    [HttpPost("case")]
    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitCase([FromBody] SubmitPublicCaseCommand command, CancellationToken ct)
    {
        var result = await _casesService.SubmitPublicCaseAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("track_complaints")]
    [HttpPost("track")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackComplaint([FromBody] TrackComplaintInput? jsonBody, [FromForm] TrackComplaintInput? formBody, [FromQuery] string? trackingNumber, CancellationToken ct)
    {
        var code = jsonBody?.TrackingNumber ?? jsonBody?.TrackCode 
                ?? formBody?.TrackingNumber ?? formBody?.TrackCode 
                ?? trackingNumber ?? string.Empty;

        var result = await _casesService.TrackComplaintAsync(new TrackComplaintQuery(code), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("track/{trackingNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackComplaintByGet(string trackingNumber, CancellationToken ct)
    {
        var result = await _casesService.TrackComplaintAsync(new TrackComplaintQuery(trackingNumber), ct);
        return StatusCode(result.StatusCode, result);
    }
}
