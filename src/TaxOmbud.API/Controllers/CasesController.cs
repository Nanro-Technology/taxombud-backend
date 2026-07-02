using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Handles operational queues and the 6-stage case routing lifecycle
/// (lodge → verify → B1 → B2 → B3 recommendation → CE approval → closed).
/// </summary>
[ApiController]
[Route("api/v1/cases")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class CasesController : ControllerBase
{
    private readonly ICasesService _casesService;

    public CasesController(ICasesService casesService)
    {
        _casesService = casesService;
    }

    /// <summary>Get paginated, filterable list of all cases.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PagedResult<CaseListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCases(
        [FromQuery] string? search,
        [FromQuery] string? stage,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _casesService.GetCasesAsync(new GetCasesQuery(search, stage, status, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get current officer's assigned cases.</summary>
    [HttpGet("my")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PagedResult<CaseListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCases(
        [FromQuery] string? search,
        [FromQuery] string? stage,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _casesService.GetMyCasesAsync(new GetMyCasesQuery(search, stage, status, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single case by ID with full details, findings, recommendations, and status history.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<CaseDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaseById(Guid id, CancellationToken ct)
    {
        var result = await _casesService.GetCaseByIdAsync(new GetCaseByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of overdue cases (past SLA deadline).</summary>
    [HttpGet("overdue")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PagedResult<CaseListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueCases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _casesService.GetOverdueCasesAsync(new GetOverdueCasesQuery(page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Query cases by 6-stage queue name (input, verify, b1, b2, b3, approval, closed).</summary>
    [HttpGet("queues/{queueName}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<QueueResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueue(
        string queueName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _casesService.GetQueueAsync(new GetQueueQuery(queueName, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get findings for a case.</summary>
    [HttpGet("{id:guid}/findings")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<CaseFindingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFindings(Guid id, CancellationToken ct)
    {
        var result = await _casesService.GetCaseFindingsAsync(new GetCaseFindingsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Add a finding to a case.</summary>
    [HttpPost("{id:guid}/findings")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFinding(Guid id, [FromBody] AddCaseFindingRequest request, CancellationToken ct)
    {
        var result = await _casesService.AddCaseFindingAsync(new AddCaseFindingCommand(id, request.Description), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update a case finding.</summary>
    [HttpPut("{id:guid}/findings/{findingId:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFinding(Guid id, Guid findingId, [FromBody] UpdateCaseFindingRequest request, CancellationToken ct)
    {
        var result = await _casesService.UpdateCaseFindingAsync(new UpdateCaseFindingCommand(id, findingId, request.Description), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get milestones for a case.</summary>
    [HttpGet("{id:guid}/milestones")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<CaseMilestoneDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMilestones(Guid id, CancellationToken ct)
    {
        var result = await _casesService.GetCaseMilestonesAsync(new GetCaseMilestonesQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Complete a milestone.</summary>
    [HttpPatch("{id:guid}/milestones/{milestoneId:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteMilestone(Guid id, Guid milestoneId, CancellationToken ct)
    {
        var result = await _casesService.CompleteMilestoneAsync(new CompleteMilestoneCommand(id, milestoneId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get communications log for a case.</summary>
    [HttpGet("{id:guid}/communications")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<CaseCommunicationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunications(Guid id, CancellationToken ct)
    {
        var result = await _casesService.GetCaseCommunicationsAsync(new GetCaseCommunicationsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get documents for a case.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<CaseDocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _casesService.GetCaseDocumentsAsync(new GetCaseDocumentsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Upload a document for a case.</summary>
    [HttpPost("{id:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await _casesService.UploadCaseDocumentAsync(new UploadCaseDocumentCommand(id, file), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Add note or internal comment on a case.</summary>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest request, CancellationToken ct)
    {
        var result = await _casesService.AddCaseNoteAsync(new AddCaseNoteCommand(id, request.Text, request.IsExternal), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Assign a case to an officer.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCase(Guid id, [FromBody] AssignCaseRequest request, CancellationToken ct)
    {
        var result = await _casesService.AssignCaseAsync(new AssignCaseCommand(id, request.OfficerId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Transition case to next stage (verify, b1, b2, b3, approval, closed).</summary>
    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionCase(Guid id, [FromBody] TransitionCaseRequest request, CancellationToken ct)
    {
        var result = await _casesService.TransitionCaseAsync(new TransitionCaseCommand(id, request.TargetStage, request.Reason), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Post B3 recommendation for final sign-off (Stage 5 → Stage 6).</summary>
    [HttpPost("{id:guid}/recommendation")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PostRecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostRecommendation(Guid id, [FromBody] PostRecommendationRequest request, CancellationToken ct)
    {
        var result = await _casesService.PostRecommendationAsync(new PostRecommendationCommand(id, request.RecommendationText), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>CE terminal approval (Stage 6) — requires written rationale of ≥ 100 characters.</summary>
    [HttpPost("{id:guid}/approvals")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveClosure(Guid id, [FromBody] ApproveClosureRequest request, CancellationToken ct)
    {
        var result = await _casesService.ApproveClosureAsync(new ApproveClosureCommand(id, request.Approve, request.Rationale), ct);
        return StatusCode(result.StatusCode, result);
    }
}
