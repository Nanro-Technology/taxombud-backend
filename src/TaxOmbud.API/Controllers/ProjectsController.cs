using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Operations.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProjectsController : ControllerBase
{
    private readonly IOperationsService _operationsService;

    public ProjectsController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects([FromQuery] GetProjectsQueries query, CancellationToken ct)
    {
        var result = await _operationsService.GetProjectsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommands command, CancellationToken ct)
    {
        var result = await _operationsService.CreateProjectAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _operationsService.UpdateProjectAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct)
    {
        var result = await _operationsService.DeleteProjectAsync(new DeleteProjectCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
