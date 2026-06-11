using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Operations.Queries.GetProjects;
using TaxOmbud.Application.Features.Operations.Commands.CreateProject;
using TaxOmbud.Application.Features.Operations.Commands.UpdateProjectStatus;

namespace TaxOmbud.Api.Controllers;

public class ProjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public ProjectsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] GetProjectsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateProjectStatus([FromBody] UpdateProjectStatusCommands command) => ToActionResult(await _mediator.Send(command));
}