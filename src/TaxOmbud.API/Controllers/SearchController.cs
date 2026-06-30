using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Search.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Global unified search across entities.
/// </summary>
[ApiController]
[Route("api/v1/search")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "OfficerOrAbove")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>Perform a global search across Complaints, Cases, and Taxpayers.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GlobalSearch([FromQuery] string query, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await _searchService.GlobalSearchAsync(new GlobalSearchQuery(query, top), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Search complaints specifically.</summary>
    [HttpGet("complaints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchComplaints([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _searchService.SearchComplaintsAsync(new SearchComplaintsQuery(query), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Search cases specifically.</summary>
    [HttpGet("cases")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCases([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _searchService.SearchCasesAsync(new SearchCasesQuery(query), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Search taxpayers specifically.</summary>
    [HttpGet("taxpayers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTaxpayers([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _searchService.SearchTaxpayersAsync(new SearchTaxpayersQuery(query), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Search documents specifically.</summary>
    [HttpGet("documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDocuments([FromQuery] string query, CancellationToken ct = default)
    {
        var result = await _searchService.SearchDocumentsAsync(new SearchDocumentsQuery(query), ct);
        return StatusCode(result.StatusCode, result);
    }
}
