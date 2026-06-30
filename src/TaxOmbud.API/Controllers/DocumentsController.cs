using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Documents.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

public record CreateDocumentRequest(
    string FileName, string FilePath, string ContentType, long FileSize, string EntityType, Guid EntityId);
public record AddDocumentVersionRequest(string FilePath);
public record ClassifyDocumentRequest(string Classification);

/// <summary>
/// Manage file uploads, document metadata, versioning, and entity-linked attachments.
/// </summary>
[ApiController]
[Route("api/v1/documents")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentsService _documentsService;

    public DocumentsController(IDocumentsService documentsService)
    {
        _documentsService = documentsService;
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
        var result = await _documentsService.GetDocumentsAsync(new GetDocumentsQuery(entityId, entityType, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single document by ID including all versions.</summary>
    [HttpGet("{id:guid}", Name = "GetDocumentById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(Guid id, CancellationToken ct)
    {
        var result = await _documentsService.GetDocumentByIdAsync(new GetDocumentByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a short-lived pre-signed download URL for a document.</summary>
    [HttpGet("{id:guid}/download-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
    {
        var result = await _documentsService.GetDownloadUrlAsync(new GetDownloadUrlQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get all versions of a specific document.</summary>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentVersions(Guid id, CancellationToken ct)
    {
        var result = await _documentsService.GetDocumentVersionsAsync(new GetDocumentVersionsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Register document metadata (after client has uploaded the file to storage).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request, CancellationToken ct)
    {
        var result = await _documentsService.CreateDocumentAsync(new CreateDocumentCommand(
            request.FileName, request.FilePath, request.ContentType, request.FileSize, request.EntityType, request.EntityId), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Add a new version to an existing document.</summary>
    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddVersion(Guid id, [FromBody] AddDocumentVersionRequest request, CancellationToken ct)
    {
        var result = await _documentsService.AddDocumentVersionAsync(new AddDocumentVersionCommand(id, request.FilePath), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetDocumentById), new { id }, result);
    }

    /// <summary>Classify or re-classify a document (e.g. Evidence, Invoice).</summary>
    [HttpPatch("{id:guid}/classify")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClassifyDocument(Guid id, [FromBody] ClassifyDocumentRequest request, CancellationToken ct)
    {
        var result = await _documentsService.ClassifyDocumentAsync(new ClassifyDocumentCommand(id, request.Classification), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a document record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
    {
        var result = await _documentsService.DeleteDocumentAsync(new DeleteDocumentCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
