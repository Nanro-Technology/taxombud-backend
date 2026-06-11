using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Lookups.Queries.GetLookups;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Reference data and lookups for dropdowns and metadata.
/// </summary>
[Authorize]
[Route("api/v1/lookups")]
public class LookupsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get lookup values by type (e.g. LeaveTypes, ComplaintCategories, TaxTypes).</summary>
    [HttpGet("{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLookups(string type, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLookupsQuery(type), ct);
        return ToActionResult(result);
    }
}
