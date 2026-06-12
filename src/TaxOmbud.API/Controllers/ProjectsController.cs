using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/projects")]
public class ProjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult GetProjects(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    public IActionResult CreateProject(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}