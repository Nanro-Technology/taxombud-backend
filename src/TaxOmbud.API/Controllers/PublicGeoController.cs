using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Geo.Queries;

namespace TaxOmbud.Api.Controllers;

[AllowAnonymous]
public class PublicGeoController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicGeoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/public/geo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGeoData([FromQuery] string action, [FromQuery(Name = "country_id")] string? countryId, CancellationToken ct)
    {
        if (action == "countries")
        {
            var result = await _mediator.Send(new GetCountriesQuery(), ct);
            return ToActionResult(result);
        }
        else if (action == "states")
        {
            var result = await _mediator.Send(new GetStatesQuery(countryId ?? string.Empty), ct);
            return ToActionResult(result);
        }

        return BadRequest("Invalid action specified.");
    }
}
