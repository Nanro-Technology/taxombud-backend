using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Complaint lifecycle management — submission, retrieval, assignment, escalation and closure.
/// </summary>
[ApiController]
[Route("api/v1/complaints")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintsService _complaintsService;

    public ComplaintsController(IComplaintsService complaintsService)
    {
        _complaintsService = complaintsService;
    }

    /// <summary>Get a paginated, filterable list of complaints.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PagedResult<ComplaintSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? taxType = null,
        [FromQuery] Guid? taxpayerId = null,
        [FromQuery] Guid? officerId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _complaintsService.GetComplaintsAsync(new GetComplaintsQuery(page, pageSize, status, taxType, taxpayerId, officerId, search), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single complaint by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetComplaintById")]
    [ProducesResponseType(typeof(Response<ComplaintDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.GetComplaintByIdAsync(new GetComplaintByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a complaint by its reference number.</summary>
    [HttpGet("reference/{refNo}")]
    [ProducesResponseType(typeof(Response<ComplaintDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string refNo, CancellationToken ct)
    {
        var result = await _complaintsService.GetComplaintByReferenceAsync(new GetComplaintByReferenceQuery(refNo), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get paginated list of complaints lodged by the current taxpayer.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(Response<PagedResult<ComplaintSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyComplaints(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _complaintsService.GetMyComplaintsAsync(new GetMyComplaintsQuery(search, status, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Submit a new complaint (taxpayer action).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireAuthenticated")]
    [ProducesResponseType(typeof(Response<SubmitComplaintResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit([FromBody] SubmitComplaintCommand command, CancellationToken ct)
    {
        var result = await _complaintsService.SubmitComplaintAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtRoute("GetComplaintById", new { id = result.Data!.ComplaintId }, result);
    }

    /// <summary>Update a complaint (only allowed in Draft status).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComplaintRequest request, CancellationToken ct)
    {
        var command = new UpdateComplaintCommand(
            id, request.Subject, request.Description, request.TaxType,
            request.TaxPeriod, request.ComplaintCategory, request.TaxOfficeRef,
            request.TinNumber, request.Priority);
        var result = await _complaintsService.UpdateComplaintAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a complaint.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.DeleteComplaintAsync(new DeleteComplaintCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update the status of a complaint.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateComplaintStatusRequest request, CancellationToken ct)
    {
        var result = await _complaintsService.UpdateComplaintStatusAsync(new UpdateComplaintStatusCommand(id, request.Status, request.Reason), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Assign a complaint to an officer.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignComplaintRequest request, CancellationToken ct)
    {
        var assignedBy = GetUserId();
        var result = await _complaintsService.AssignComplaintAsync(new AssignComplaintCommand(id, request.OfficerId, assignedBy), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Escalate a complaint.</summary>
    [HttpPost("{id:guid}/escalate")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] EscalateComplaintRequest request, CancellationToken ct)
    {
        var escalatedBy = GetUserId();
        var result = await _complaintsService.EscalateComplaintAsync(new EscalateComplaintCommand(id, request.Reason, escalatedBy), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Close a complaint.</summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseComplaintRequest request, CancellationToken ct)
    {
        var closedBy = GetUserId();
        var result = await _complaintsService.CloseComplaintAsync(new CloseComplaintCommand(id, request.Reason, closedBy), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Reopen a closed complaint.</summary>
    [HttpPost("{id:guid}/reopen")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct)
    {
        var reopenedBy = GetUserId();
        var result = await _complaintsService.ReopenComplaintAsync(new ReopenComplaintCommand(id, reopenedBy), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a timeline of events for a complaint.</summary>
    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<TimelineEventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.GetComplaintTimelineAsync(new GetComplaintTimelineQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get notes added to a complaint.</summary>
    [HttpGet("{id:guid}/notes")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<ComplaintNoteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotes(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.GetComplaintNotesAsync(new GetComplaintNotesQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Add a note to a complaint.</summary>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddComplaintNoteRequest request, CancellationToken ct)
    {
        var authorId = GetUserId();
        var result = await _complaintsService.AddComplaintNoteAsync(new AddComplaintNoteCommand(id, request.Body, request.Visibility, authorId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get documents uploaded for a complaint.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<ComplaintDocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.GetComplaintDocumentsAsync(new GetComplaintDocumentsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Upload a document for a complaint.</summary>
    [HttpPost("{id:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await _complaintsService.UploadComplaintDocumentAsync(new UploadComplaintDocumentCommand(id, file), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get related complaints.</summary>
    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<RelatedComplaintDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelated(Guid id, CancellationToken ct)
    {
        var result = await _complaintsService.GetRelatedComplaintsAsync(new GetRelatedComplaintsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Link this complaint to another complaint.</summary>
    [HttpPost("{id:guid}/link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Link(Guid id, [FromBody] LinkComplaintRequest request, CancellationToken ct)
    {
        var result = await _complaintsService.LinkComplaintsAsync(new LinkComplaintsCommand(id, request.TargetComplaintId, request.LinkType), ct);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
