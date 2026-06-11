using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Search.Queries.GlobalSearch;
using TaxOmbud.Application.Features.Search.Queries.SearchCases;
using TaxOmbud.Application.Features.Search.Queries.SearchComplaints;
using TaxOmbud.Application.Features.Search.Queries.SearchDocuments;
using TaxOmbud.Application.Features.Search.Queries.SearchTaxpayers;

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

    /// <summary>Search complaints specifically.</summary>
    [HttpGet("complaints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchComplaints([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchComplaintsQuery(query), ct);
        return ToActionResult(result);
    }

    /// <summary>Search cases specifically.</summary>
    [HttpGet("cases")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCases([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchCasesQuery(query), ct);
        return ToActionResult(result);
    }

    /// <summary>Search taxpayers specifically.</summary>
    [HttpGet("taxpayers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTaxpayers([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchTaxpayersQuery(query), ct);
        return ToActionResult(result);
    }

    /// <summary>Search documents specifically.</summary>
    [HttpGet("documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDocuments([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchDocumentsQuery(query), ct);
        return ToActionResult(result);
    }
}
