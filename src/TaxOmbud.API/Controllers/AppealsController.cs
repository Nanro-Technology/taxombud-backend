using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Appeals.Commands.FileAppeal;
using TaxOmbud.Application.Features.Appeals.Commands.ReviewAppeal;
using TaxOmbud.Application.Features.Appeals.Commands.UploadAppealDocument;
using TaxOmbud.Application.Features.Appeals.Queries.GetAppealById;
using TaxOmbud.Application.Features.Appeals.Queries.GetAppealDocuments;
using TaxOmbud.Application.Features.Appeals.Queries.GetAppeals;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Allows taxpayers to appeal decisions on closed cases, and officers to review appeal cases.
/// </summary>
[Authorize]
[Route("api/v1/appeals")]
public class AppealsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AppealsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List all submitted taxpayer decision appeals (Officer and above only).</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> GetAppeals(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAppealsQuery(status, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get details of an appeal by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppealById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAppealByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>File a new decision appeal for a closed case (Taxpayer action).</summary>
    [HttpPost]
    [Authorize(Policy = "TaxpayerOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileAppeal([FromBody] FileAppealRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new FileAppealCommand(request.CaseId, request.Reason), ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetAppealById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Review, uphold, or dismiss a decision appeal (Officer/Director action).</summary>
    [HttpPost("{id:guid}/review")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewAppeal(Guid id, [FromBody] ReviewAppealRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReviewAppealCommand(id, request.Action, request.Notes), ct);
        return ToActionResult(result);
    }

    /// <summary>Get documents attached to an appeal.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAppealDocumentsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload a supporting document for an appeal.</summary>
    [HttpPost("{id:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await _mediator.Send(new UploadAppealDocumentCommand(id, file), ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetDocuments), new { id }, result.Value);
    }
}

public record FileAppealRequest(Guid CaseId, string Reason);
public record ReviewAppealRequest(string Action, string Notes);
