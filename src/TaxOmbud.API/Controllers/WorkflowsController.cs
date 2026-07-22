using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Workflows.Commands;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Queries;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/workflows")]
[Tags("Workflows")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<WorkflowDto>>>> GetWorkflows([FromQuery] string? category, [FromQuery] bool? isActive)
    {
        var result = await _mediator.Send(new GetWorkflowsQuery(category, isActive));
        return Ok(new Response<List<WorkflowDto>> { StatusCode = 200, Message = "Workflows retrieved successfully", Data = result });
    }

    [HttpPost]
    public async Task<ActionResult<Response<WorkflowDto>>> CreateWorkflow([FromBody] CreateWorkflowCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetWorkflows), new { id = result.Id }, new Response<WorkflowDto> { StatusCode = 201, Message = "Workflow created successfully", Data = result });
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<Response<WorkflowVersionDto>>> PublishWorkflow(Guid id)
    {
        var result = await _mediator.Send(new PublishWorkflowCommand(id));
        return Ok(new Response<WorkflowVersionDto> { StatusCode = 200, Message = "Workflow version published successfully", Data = result });
    }
}
