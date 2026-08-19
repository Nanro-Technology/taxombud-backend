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

    [HttpPost("{complaintId}/register")]
    public async Task<ActionResult<Response<bool>>> RegisterComplaint(Guid complaintId)
    {
        var result = await _stageService.RegisterComplaintAsync(complaintId, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Complaint formally registered.", Data = result });
    }

    [HttpPost("{caseId}/assess")]
    public async Task<ActionResult<Response<bool>>> AssessAdmissibility(Guid caseId, [FromBody] AdmissibilityAssessmentDto dto)
    {
        var result = await _stageService.AssessAdmissibilityAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Admissibility assessment saved.", Data = result });
    }

    [HttpPost("{caseId}/assign")]
    public async Task<ActionResult<Response<bool>>> AssignCase(Guid caseId, [FromBody] AssignCaseRequest request)
    {
        var result = await _stageService.AssignCaseByCeAsync(caseId, request.OfficerId, request.DepartmentId, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Case assigned successfully.", Data = result });
    }

    [HttpPost("{caseId}/mediation")]
    public async Task<ActionResult<Response<bool>>> LogMediationSession(Guid caseId, [FromBody] MediationLogDto dto)
    {
        var result = await _stageService.LogMediationSessionAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Mediation session logged.", Data = result });
    }

    [HttpPost("{caseId}/qa-review")]
    public async Task<ActionResult<Response<bool>>> SubmitQaReview(Guid caseId, [FromBody] QualityAssuranceReviewDto dto)
    {
        var result = await _stageService.SubmitQaReviewAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "QA review submitted.", Data = result });
    }

    [HttpPost("{caseId}/decision")]
    public async Task<ActionResult<Response<bool>>> IssueDecision(Guid caseId, [FromBody] CaseDecisionDto dto)
    {
        var result = await _stageService.IssueCeDecisionAsync(caseId, dto, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Chief Executive decision issued.", Data = result });
    }

    [HttpPost("{caseId}/close")]
    public async Task<ActionResult<Response<bool>>> CloseAndArchive(Guid caseId, [FromBody] CloseCaseRequest request)
    {
        var result = await _stageService.CloseAndArchiveCaseAsync(caseId, request.Outcome, request.Summary, GetUserId());
        return Ok(new Response<bool> { StatusCode = 200, Message = "Case closed and archived.", Data = result });
    }

    [HttpPost("call-center")]
    public async Task<ActionResult<Response<Guid>>> LogCallCenterRecord([FromBody] CallCenterRecordDto dto)
    {
        var recordId = await _stageService.LogCallCenterRecordAsync(dto, GetUserId());
        return Ok(new Response<Guid> { StatusCode = 200, Message = "Call center record saved.", Data = recordId });
    }

    /// <summary>Record a Case Officer finding (Stage 7) — delegates to ICasesService.</summary>
    [HttpPost("{caseId}/findings")]
    public async Task<ActionResult> AddFinding(Guid caseId, [FromBody] AddFindingRequest request)
    {
        var result = await _casesService.AddCaseFindingAsync(
            new AddCaseFindingCommand(caseId, request.Description),
            HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Retrieve all findings for a case — delegates to ICasesService.</summary>
    [HttpGet("{caseId}/findings")]
    public async Task<ActionResult<Response<IReadOnlyList<CaseFindingDto>>>> GetFindings(Guid caseId)
    {
        var result = await _casesService.GetCaseFindingsAsync(
            new GetCaseFindingsQuery(caseId),
            HttpContext.RequestAborted);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{caseId}/details")]
    public async Task<ActionResult<Response<WorkflowStageDetailsDto>>> GetWorkflowStageDetails(Guid caseId)
    {
        var details = await _stageService.GetWorkflowStageDetailsAsync(caseId);
        if (details == null) return NotFound(new Response<WorkflowStageDetailsDto> { StatusCode = 404, Message = "Case details not found." });
        return Ok(new Response<WorkflowStageDetailsDto> { StatusCode = 200, Message = "Stage details retrieved.", Data = details });
    }
}

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