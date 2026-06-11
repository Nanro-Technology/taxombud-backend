using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Cases.Commands.AddCaseFinding;
using TaxOmbud.Application.Features.Cases.Commands.AddCaseNote;
using TaxOmbud.Application.Features.Cases.Commands.ApproveClosure;
using TaxOmbud.Application.Features.Cases.Commands.AssignCase;
using TaxOmbud.Application.Features.Cases.Commands.CompleteMilestone;
using TaxOmbud.Application.Features.Cases.Commands.PostRecommendation;
using TaxOmbud.Application.Features.Cases.Commands.TransitionCase;
using TaxOmbud.Application.Features.Cases.Commands.UpdateCaseFinding;
using TaxOmbud.Application.Features.Cases.Commands.UploadCaseDocument;
using TaxOmbud.Application.Features.Cases.Queries.GetCaseById;
using TaxOmbud.Application.Features.Cases.Queries.GetCaseCommunications;
using TaxOmbud.Application.Features.Cases.Queries.GetCaseDocuments;
using TaxOmbud.Application.Features.Cases.Queries.GetCaseFindings;
using TaxOmbud.Application.Features.Cases.Queries.GetCaseMilestones;
using TaxOmbud.Application.Features.Cases.Queries.GetCases;
using TaxOmbud.Application.Features.Cases.Queries.GetMyCases;
using TaxOmbud.Application.Features.Cases.Queries.GetQueue;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Handles operational queues and the 6-stage case routing lifecycle (lodge, verify, B1, B2, B3 recommendation, CE approval).
/// </summary>
[Authorize]
[Route("api/v1/cases")]
public class CasesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get list of all cases.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> GetCases(
        [FromQuery] string? search,
        [FromQuery] string? stage,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCasesQuery(search, stage, status, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get current officer's assigned cases.</summary>
    [HttpGet("my")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(PagedResult<CaseListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCases(
        [FromQuery] string? search,
        [FromQuery] string? stage,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyCasesQuery(search, stage, status, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single case by ID with full details, findings, recommendations, and status history.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaseById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaseByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Query cases by 6-stage queue name (e.g. input, verify, b1, b2, b3, approval, closed).</summary>
    [HttpGet("queues/{queueName}")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> GetQueue(
        string queueName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetQueueQuery(queueName, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Add note or internal comment on a complaint/case.</summary>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddCaseNoteCommand(id, request.Text, request.IsExternal), ct);
        return ToActionResult(result);
    }

    /// <summary>Assign a complaint or case to an officer.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCase(Guid id, [FromBody] AssignCaseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignCaseCommand(id, request.OfficerId), ct);
        return ToActionResult(result);
    }

    /// <summary>Transition case to next stage (lodge, verify, B1, B2, B3 recommendation, CE approval, closed).</summary>
    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionCase(Guid id, [FromBody] TransitionCaseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new TransitionCaseCommand(id, request.TargetStage, request.Reason), ct);
        return ToActionResult(result);
    }

    /// <summary>Post B3 recommendation for final sign-off (Stage 5 to Stage 6).</summary>
    [HttpPost("{id:guid}/recommendation")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostRecommendation(Guid id, [FromBody] PostRecommendationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new PostRecommendationCommand(id, request.RecommendationText), ct);
        return ToActionResult(result);
    }

    /// <summary>CE terminal approval (Stage 6) requiring written rationale of >= 100 characters.</summary>
    [HttpPost("{id:guid}/approvals")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveClosure(Guid id, [FromBody] ApproveClosureRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveClosureCommand(id, request.Approve, request.Rationale), ct);
        return ToActionResult(result);
    }

    /// <summary>Get findings for a case.</summary>
    [HttpGet("{id:guid}/findings")]
    [ProducesResponseType(typeof(IReadOnlyList<CaseFindingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFindings(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaseFindingsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Add a finding to a case.</summary>
    [HttpPost("{id:guid}/findings")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFinding(Guid id, [FromBody] AddCaseFindingRequest request, CancellationToken ct)
    {
        var command = new AddCaseFindingCommand(id, request.Description);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Update a case finding.</summary>
    [HttpPut("{id:guid}/findings/{findingId:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFinding(Guid id, Guid findingId, [FromBody] UpdateCaseFindingRequest request, CancellationToken ct)
    {
        var command = new UpdateCaseFindingCommand(id, findingId, request.Description);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Get communications log for a case.</summary>
    [HttpGet("{id:guid}/communications")]
    [ProducesResponseType(typeof(IReadOnlyList<CaseCommunicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunications(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaseCommunicationsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get documents for a case.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(typeof(IReadOnlyList<CaseDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaseDocumentsQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload a document for a case.</summary>
    [HttpPost("{id:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        var command = new UploadCaseDocumentCommand(id, file);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Get milestones for a case.</summary>
    [HttpGet("{id:guid}/milestones")]
    [ProducesResponseType(typeof(IReadOnlyList<CaseMilestoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMilestones(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaseMilestonesQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Complete a milestone.</summary>
    [HttpPatch("{id:guid}/milestones/{milestoneId:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteMilestone(Guid id, Guid milestoneId, CancellationToken ct)
    {
        var command = new CompleteMilestoneCommand(id, milestoneId);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}

public record AddNoteRequest(string Text, bool IsExternal);
public record AssignCaseRequest(Guid OfficerId);
public record TransitionCaseRequest(string TargetStage, string? Reason);
public record PostRecommendationRequest(string RecommendationText);
public record ApproveClosureRequest(bool Approve, string Rationale);
public record AddCaseFindingRequest(string Description);
public record UpdateCaseFindingRequest(string Description);
