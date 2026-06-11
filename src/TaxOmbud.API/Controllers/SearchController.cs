using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Search.Queries.GlobalSearch;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Global unified search across entities.
/// </summary>
[Authorize(Policy = "OfficerOrAbove")]
[Route("api/v1/search")]
public class SearchController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Perform a global search across Complaints, Cases, and Taxpayers.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GlobalSearch([FromQuery] string query, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GlobalSearchQuery(query, top), ct);
        return ToActionResult(result);
    }
}
