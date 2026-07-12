using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Tasks.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TasksController : ControllerBase
{
    private readonly ITasksService _tasksService;

    public TasksController(ITasksService tasksService)
    {
        _tasksService = tasksService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(CancellationToken ct)
    {
        var result = await _tasksService.GetCaseTasksAsync(new GetCaseTasksQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _tasksService.GetCaseTaskByIdAsync(new GetCaseTaskByIdQuery(id), ct);
            return Ok(result);
        }
        catch (TaxOmbud.Common.CustomException.NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateCaseTaskCommand command, CancellationToken ct)
    {
        var result = await _tasksService.CreateCaseTaskAsync(command, ct);
        return Ok(new { StatusCode = 200, Message = "Task created successfully.", Data = result });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateCaseTaskCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }

        try
        {
            await _tasksService.UpdateCaseTaskAsync(command, ct);
            return Ok(new { StatusCode = 200, Message = "Task updated successfully." });
        }
        catch (TaxOmbud.Common.CustomException.NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        try
        {
            await _tasksService.DeleteCaseTaskAsync(new DeleteCaseTaskCommand(id), ct);
            return Ok(new { StatusCode = 200, Message = "Task deleted successfully." });
        }
        catch (TaxOmbud.Common.CustomException.NotFoundException)
        {
            return NotFound();
        }
    }
}
