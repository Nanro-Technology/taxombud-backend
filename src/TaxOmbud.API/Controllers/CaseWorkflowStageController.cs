using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/workflow-stages")]
[Tags("Workflow Pipeline Stages")]
[Authorize]
public class CaseWorkflowStageController : ControllerBase
{
    private readonly ICaseWorkflowStageService _stageService;
    private readonly ICasesService _casesService;

    public CaseWorkflowStageController(ICaseWorkflowStageService stageService, ICasesService casesService)
    {
        _stageService = stageService;
        _casesService = casesService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // ─── Stage 2: Registration & Acknowledgement ─────────────────────────────

    /// <summary>Stage 2 - Formally registers a submitted complaint and assigns a Case Reference Number.</summary>
    [HttpPost("{complaintId}/register")]
    public async Task<ActionResult<Response<bool>>> RegisterComplaint(Guid complaintId)
    {
        var result = await _stageService.RegisterComplaintAsync(complaintId, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Complaint formally registered - Stage 2 complete.", Data = result });
    }

    // ─── Stage 3: Initial Review & Assignment ────────────────────────────────

    /// <summary>Stage 3 - CE performs Initial Review and optionally assigns officer/department.</summary>
    [HttpPost("{caseId}/initial-review")]
    public async Task<ActionResult<Response<bool>>> StartInitialReview(Guid caseId, [FromBody] StartInitialReviewRequest request)
    {
        var cmd = new StartInitialReviewCommand(caseId, request.AssignedOfficerId, request.DepartmentId);
        var result = await _stageService.StartInitialReviewAsync(cmd, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Initial review recorded - Stage 3 complete.", Data = result });
    }

    // ─── Stage 4: Jurisdiction & Admissibility Assessment ───────────────────

    /// <summary>Stage 4 - Assess admissibility of the complaint. If admissible, advances to Stage 5.</summary>
    [HttpPost("{caseId}/assess")]
    public async Task<ActionResult<Response<bool>>> AssessAdmissibility(Guid caseId, [FromBody] AdmissibilityAssessmentDto dto)
    {
        var result = await _stageService.AssessAdmissibilityAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Admissibility assessment saved - Stage 4.", Data = result });
    }

    /// <summary>
    /// Stage 4 terminal - Formally declares a complaint NOT ADMISSIBLE.
    /// Closes the case and dispatches a formal determination letter to the taxpayer.
    /// </summary>
    [HttpPost("{caseId}/not-admissible")]
    public async Task<ActionResult<Response<bool>>> DeclareNotAdmissible(Guid caseId, [FromBody] DeclareNotAdmissibleRequest request)
    {
        var cmd = new DeclareNotAdmissibleCommand(caseId, request.Reason, request.AdmissibilityAssessmentId);
        var result = await _stageService.DeclareNotAdmissibleAsync(cmd, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Complaint formally declared NOT ADMISSIBLE. Taxpayer notified.", Data = result });
    }

    // ─── Stage 3 (CE Assignment path) ────────────────────────────────────────

    /// <summary>CE assigns case to a specific officer and department.</summary>
    [HttpPost("{caseId}/assign")]
    public async Task<ActionResult<Response<bool>>> AssignCase(Guid caseId, [FromBody] AssignCaseRequest request)
    {
        var result = await _stageService.AssignCaseByCeAsync(caseId, request.OfficerId, request.DepartmentId, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Case assigned successfully.", Data = result });
    }

    // ─── Stage 5: Investigation & Resolution ─────────────────────────────────

    /// <summary>Stage 5 - Logs a dispute resolution / mediation session.</summary>
    [HttpPost("{caseId}/mediation")]
    public async Task<ActionResult<Response<bool>>> LogMediationSession(Guid caseId, [FromBody] MediationLogDto dto)
    {
        var result = await _stageService.LogMediationSessionAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Mediation session logged.", Data = result });
    }

    /// <summary>Stage 5 QA - Supervisor submits Quality Assurance review of investigation report.</summary>
    [HttpPost("{caseId}/qa-review")]
    public async Task<ActionResult<Response<bool>>> SubmitQaReview(Guid caseId, [FromBody] QualityAssuranceReviewDto dto)
    {
        var result = await _stageService.SubmitQaReviewAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "QA review submitted.", Data = result });
    }

    // ─── Stage 6: Decision & Communication ───────────────────────────────────

    /// <summary>Stage 6 - Chief Executive issues the formal case decision and communication.</summary>
    [HttpPost("{caseId}/decision")]
    public async Task<ActionResult<Response<bool>>> IssueDecision(Guid caseId, [FromBody] CaseDecisionDto dto)
    {
        var result = await _stageService.IssueCeDecisionAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Chief Executive decision issued - Stage 6 complete.", Data = result });
    }

    // ─── Stage 7: Closure & Archiving ────────────────────────────────────────

    /// <summary>Stage 7 - Close the case with final outcome and summary.</summary>
    [HttpPost("{caseId}/close")]
    public async Task<ActionResult<Response<bool>>> CloseAndArchive(Guid caseId, [FromBody] CloseCaseRequest request)
    {
        var result = await _stageService.CloseAndArchiveCaseAsync(caseId, request.Outcome, request.Summary, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Case closed.", Data = result });
    }

    /// <summary>Stage 7 - Registry formally archives a closed case and creates an archive record.</summary>
    [HttpPost("{caseId}/archive")]
    public async Task<ActionResult<Response<bool>>> ArchiveCase(Guid caseId, [FromBody] ArchiveCaseRequest request)
    {
        var cmd = new ArchiveCaseCommand(caseId, request.ArchivePurpose, request.ArchivedDocumentRefs);
        var result = await _stageService.ArchiveCaseAsync(cmd, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Case formally archived - Stage 7 complete.", Data = result });
    }

    // ─── Supporting Intakes ───────────────────────────────────────────────────

    [HttpPost("call-center")]
    public async Task<ActionResult<Response<Guid>>> LogCallCenterRecord([FromBody] CallCenterRecordDto dto)
    {
        var recordId = await _stageService.LogCallCenterRecordAsync(dto, GetUserId());
        return Ok(new Response<Guid> { StatusCode = 200, Message = "Call center record saved.", Data = recordId });
    }

    // ─── Case Findings ────────────────────────────────────────────────────────

    /// <summary>Record a Case Officer finding during Stage 5 Investigation.</summary>
    [HttpPost("{caseId}/findings")]
    public async Task<ActionResult> AddFinding(Guid caseId, [FromBody] AddFindingRequest request)
    {
        var result = await _casesService.AddCaseFindingAsync(
            new AddCaseFindingCommand(caseId, request.Description),
            HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Retrieve all findings for a case.</summary>
    [HttpGet("{caseId}/findings")]
    public async Task<ActionResult<Response<IReadOnlyList<CaseFindingDto>>>> GetFindings(Guid caseId)
    {
        var result = await _casesService.GetCaseFindingsAsync(
            new GetCaseFindingsQuery(caseId),
            HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{caseId}/details")]
    public async Task<ActionResult<Response<WorkflowStageDetailsDto>>> GetWorkflowStageDetails(Guid caseId)
    {
        var details = await _stageService.GetWorkflowStageDetailsAsync(caseId);
        if (details == null) return NotFound(new Response<WorkflowStageDetailsDto> { StatusCode = 404, Message = "Case details not found." });
        return Ok(new Response<WorkflowStageDetailsDto> { StatusCode = 200, Message = "Stage details retrieved.", Data = details });
    }
}

// ─── Request Models ───────────────────────────────────────────────────────────

public class AddFindingRequest
{
    public string Description { get; set; } = null!;
}

public class AssignCaseRequest
{
    public Guid OfficerId { get; set; }
    public Guid DepartmentId { get; set; }
}

public class CloseCaseRequest
{
    public string Outcome { get; set; } = null!;
    public string Summary { get; set; } = null!;
}

public class StartInitialReviewRequest
{
    public Guid? AssignedOfficerId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class DeclareNotAdmissibleRequest
{
    public string Reason { get; set; } = null!;
    public Guid AdmissibilityAssessmentId { get; set; }
}

public class ArchiveCaseRequest
{
    public string ArchivePurpose { get; set; } = null!;
    public List<string> ArchivedDocumentRefs { get; set; } = new();
}
