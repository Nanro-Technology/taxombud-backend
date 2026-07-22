using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Workflows.Commands;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Queries;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/cases")]
[Tags("Case Workflows")]
[Authorize]
public class CaseWorkflowController : ControllerBase
{
    private readonly IMediator _mediator;

    public CaseWorkflowController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("submit-workflow")]
    public async Task<ActionResult<Response<WorkflowInstanceDto>>> SubmitCaseToWorkflow([FromBody] SubmitCaseToWorkflowCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new Response<WorkflowInstanceDto> { StatusCode = 200, Message = "Case submitted to workflow successfully", Data = result });
    }

    [HttpPost("execute-approval")]
    public async Task<ActionResult<Response<bool>>> ExecuteApproval([FromBody] ExecuteCaseApprovalCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new Response<bool> { StatusCode = 200, Message = "Workflow approval action executed successfully", Data = result });
    }

    [HttpPost("reassign-task")]
    public async Task<ActionResult<Response<bool>>> ReassignTask([FromBody] ReassignCaseTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new Response<bool> { StatusCode = 200, Message = "Task reassigned successfully", Data = result });
    }

    [HttpGet("pending-tasks")]
    public async Task<ActionResult<Response<List<CaseApprovalTaskDto>>>> GetPendingTasks()
    {
        var result = await _mediator.Send(new GetPendingApprovalTasksQuery());
        return Ok(new Response<List<CaseApprovalTaskDto>> { StatusCode = 200, Message = "Pending tasks retrieved successfully", Data = result });
    }

    [HttpGet("{caseId}/workflow-timeline")]
    public async Task<ActionResult<Response<List<CaseWorkflowAuditLogDto>>>> GetWorkflowTimeline(Guid caseId)
    {
        var result = await _mediator.Send(new GetCaseWorkflowTimelineQuery(caseId));
        return Ok(new Response<List<CaseWorkflowAuditLogDto>> { StatusCode = 200, Message = "Workflow timeline retrieved successfully", Data = result });
    }
}
