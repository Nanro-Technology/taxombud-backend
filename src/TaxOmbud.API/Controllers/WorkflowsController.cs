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

    [HttpPut("{id}")]
    public async Task<ActionResult<Response<WorkflowDto>>> UpdateWorkflow(Guid id, [FromBody] UpdateWorkflowCommand command)
    {
        if (id != command.Id)
        {
            command = command with { Id = id };
        }
        var result = await _mediator.Send(command);
        return Ok(new Response<WorkflowDto> { StatusCode = 200, Message = "Workflow updated successfully", Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Response<bool>>> DeleteWorkflow(Guid id)
    {
        var result = await _mediator.Send(new DeleteWorkflowCommand(id));
        return Ok(new Response<bool> { StatusCode = 200, Message = "Workflow deleted successfully", Data = result });
    }

    // ─── Stage Library ────────────────────────────────────────────────────────

    [HttpGet("stage-library")]
    public async Task<ActionResult<Response<List<WorkflowStageLibraryItemDto>>>> GetStageLibrary()
    {
        var result = await _mediator.Send(new GetStageLibraryQuery());
        return Ok(new Response<List<WorkflowStageLibraryItemDto>> { StatusCode = 200, Message = "Stage library retrieved", Data = result });
    }

    [HttpPost("stage-library")]
    public async Task<ActionResult<Response<WorkflowStageLibraryItemDto>>> CreateStageLibraryItem([FromBody] CreateStageLibraryItemCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetStageLibrary), null, new Response<WorkflowStageLibraryItemDto> { StatusCode = 201, Message = "Stage library item created", Data = result });
    }

    [HttpPut("stage-library/{id}")]
    public async Task<ActionResult<Response<WorkflowStageLibraryItemDto>>> UpdateStageLibraryItem(Guid id, [FromBody] UpdateStageLibraryItemCommand command)
    {
        if (id != command.Id) command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new Response<WorkflowStageLibraryItemDto> { StatusCode = 200, Message = "Stage library item updated", Data = result });
    }

    [HttpDelete("stage-library/{id}")]
    public async Task<ActionResult<Response<bool>>> DeleteStageLibraryItem(Guid id)
    {
        var result = await _mediator.Send(new DeleteStageLibraryItemCommand(id));
        return Ok(new Response<bool> { StatusCode = 200, Message = "Stage library item deleted", Data = result });
    }
}
