using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Complaints.Commands.AddComplaintNote;
using TaxOmbud.Application.Features.Complaints.Commands.AssignComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.CloseComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.DeleteComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.EscalateComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.LinkComplaints;
using TaxOmbud.Application.Features.Complaints.Commands.ReopenComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.SubmitComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.UpdateComplaint;
using TaxOmbud.Application.Features.Complaints.Commands.UpdateComplaintStatus;
using TaxOmbud.Application.Features.Complaints.Commands.UploadComplaintDocument;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintById;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintByReference;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintDocuments;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintNotes;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintTimeline;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaints;
using TaxOmbud.Application.Features.Complaints.Queries.GetMyComplaints;
using TaxOmbud.Application.Features.Complaints.Queries.GetRelatedComplaints;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Api.Controllers;

public record UpdateComplaintRequest(
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Priority
);

public record UpdateComplaintStatusRequest(ComplaintStatus Status, string? Reason);
public record AssignComplaintRequest(Guid OfficerId);
public record EscalateComplaintRequest(string Reason);
public record CloseComplaintRequest(string Reason);
public record AddComplaintNoteRequest(string Body, string Visibility);
public record LinkComplaintRequest(Guid TargetComplaintId, string? LinkType);

/// <summary>
/// Complaint lifecycle management — submission, retrieval, assignment, escalation and closure.
/// </summary>
[Authorize]
[Route("api/v1/complaints")]
public class ComplaintsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get a paginated, filterable list of complaints.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
        var query = new GetComplaintsQuery(page, pageSize, status, taxType, taxpayerId, officerId, search);
        var result = await _mediator.Send(query, ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single complaint by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetComplaintById")]
    [ProducesResponseType(typeof(ComplaintDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Submit a new complaint (taxpayer action).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireAuthenticated")]
    [ProducesResponseType(typeof(SubmitComplaintResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit([FromBody] SubmitComplaintCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtRoute("GetComplaintById", new { id = result.Value!.ComplaintId }, result.Value);
    }

    /// <summary>Get a complaint by its reference number.</summary>
    [HttpGet("reference/{refNo}")]
    [ProducesResponseType(typeof(ComplaintDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string refNo, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintByReferenceQuery(refNo), ct);
        return ToActionResult(result);
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
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a complaint.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteComplaintCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Update the status of a complaint.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateComplaintStatusRequest request, CancellationToken ct)
    {
        var command = new UpdateComplaintStatusCommand(id, request.Status, request.Reason);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Assign a complaint to an officer.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignComplaintRequest request, CancellationToken ct)
    {
        var assignedBy = GetUserId();
        var command = new AssignComplaintCommand(id, request.OfficerId, assignedBy);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Escalate a complaint.</summary>
    [HttpPatch("{id:guid}/escalate")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] EscalateComplaintRequest request, CancellationToken ct)
    {
        var escalatedBy = GetUserId();
        var command = new EscalateComplaintCommand(id, request.Reason, escalatedBy);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Close a complaint.</summary>
    [HttpPatch("{id:guid}/close")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseComplaintRequest request, CancellationToken ct)
    {
        var closedBy = GetUserId();
        var command = new CloseComplaintCommand(id, request.Reason, closedBy);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Reopen a closed complaint.</summary>
    [HttpPatch("{id:guid}/reopen")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct)
    {
        var reopenedBy = GetUserId();
        var command = new ReopenComplaintCommand(id, reopenedBy);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Get a timeline of events for a complaint.</summary>
    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(typeof(IReadOnlyList<TimelineEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintTimelineQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get notes added to a complaint.</summary>
    [HttpGet("{id:guid}/notes")]
    [ProducesResponseType(typeof(IReadOnlyList<ComplaintNoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotes(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintNotesQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get paginated list of complaints lodged by the current taxpayer.</summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyComplaints(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyComplaintsQuery(search, status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Add a note to a complaint.</summary>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddComplaintNoteRequest request, CancellationToken ct)
    {
        var authorId = GetUserId();
        var command = new AddComplaintNoteCommand(id, request.Body, request.Visibility, authorId);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Get documents uploaded for a complaint.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(typeof(IReadOnlyList<ComplaintDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintDocumentsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload a document for a complaint.</summary>
    [HttpPost("{id:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        var command = new UploadComplaintDocumentCommand(id, file);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Get related complaints.</summary>
    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(IReadOnlyList<RelatedComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelated(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRelatedComplaintsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Link this complaint to another complaint.</summary>
    [HttpPost("{id:guid}/link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Link(Guid id, [FromBody] LinkComplaintRequest request, CancellationToken ct)
    {
        var command = new LinkComplaintsCommand(id, request.TargetComplaintId, request.LinkType);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
