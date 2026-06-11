using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Documents.Commands.AddDocumentVersion;
using TaxOmbud.Application.Features.Documents.Commands.ClassifyDocument;
using TaxOmbud.Application.Features.Documents.Commands.CreateDocument;
using TaxOmbud.Application.Features.Documents.Commands.DeleteDocument;
using TaxOmbud.Application.Features.Documents.Queries.GetDocumentById;
using TaxOmbud.Application.Features.Documents.Queries.GetDocuments;
using TaxOmbud.Application.Features.Documents.Queries.GetDocumentVersions;
using TaxOmbud.Application.Features.Documents.Queries.GetDownloadUrl;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage file uploads, document metadata, versioning, and entity-linked attachments.
/// </summary>
[Authorize]
[Route("api/v1/documents")]
public class DocumentsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List documents linked to an entity (e.g. a complaint, case, or appeal).</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] Guid? entityId,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDocumentsQuery(entityId, entityType, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single document by ID including all versions.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Register document metadata (after client has uploaded the file to storage).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDocumentCommand(
            request.FileName,
            request.FilePath,
            request.ContentType,
            request.FileSize,
            request.EntityType,
            request.EntityId
        ), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Add a new version to an existing document.</summary>
    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddVersion(Guid id, [FromBody] AddDocumentVersionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddDocumentVersionCommand(id, request.FilePath), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetDocumentById), new { id }, result.Value);
    }

    /// <summary>Get all versions of a specific document.</summary>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentVersions(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentVersionsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a short-lived pre-signed download URL for a document.</summary>
    [HttpGet("{id:guid}/download-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDownloadUrlQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Classify or re-classify a document (e.g. Evidence, Invoice).</summary>
    [HttpPatch("{id:guid}/classify")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClassifyDocument(Guid id, [FromBody] ClassifyDocumentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ClassifyDocumentCommand(id, request.Classification), ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a document record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteDocumentCommand(id), ct);
        return ToActionResult(result);
    }
}

public record CreateDocumentRequest(
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId
);

public record AddDocumentVersionRequest(string FilePath);

public record ClassifyDocumentRequest(string Classification);
