using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Lookups.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Reference data and lookups for dropdowns and metadata.
/// </summary>
[Authorize]
[Route("api/v1/lookups")]
public class LookupsController : ControllerBase
{
    private readonly ILookupsService _lookupsService;

    public LookupsController(
        ILookupsService lookupsService
    )
    {
        _lookupsService = lookupsService;
    }

    /// <summary>Get lookup values by type (e.g. LeaveTypes, ComplaintCategories, TaxTypes).</summary>
    [HttpGet("{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLookups(string type, CancellationToken ct)
    {
        var result = await _lookupsService.GetLookupsAsync(new GetLookupsQuery(type), ct);
        return StatusCode(result.StatusCode, result);
    }
}
