using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Geo.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/geo")]
public class PublicGeoController : ControllerBase
{
    private readonly IGeoService _geoService;

    public PublicGeoController(IGeoService geoService)
    {
        _geoService = geoService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGeoData([FromQuery] string action, [FromQuery(Name = "country_id")] string? countryId, CancellationToken ct)
    {
        if (action == "countries")
        {
            var result = await _geoService.GetCountriesAsync(new GetCountriesQuery(), ct);
            return StatusCode(result.StatusCode, result);
        }
        else if (action == "states")
        {
            var result = await _geoService.GetStatesAsync(new GetStatesQuery(countryId ?? string.Empty), ct);
            return StatusCode(result.StatusCode, result);
        }

        return BadRequest("Invalid action specified.");
    }
}
