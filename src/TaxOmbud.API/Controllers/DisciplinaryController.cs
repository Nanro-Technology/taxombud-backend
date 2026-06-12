using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Api.Controllers;

[Authorize]
[Route("api/v1/disciplinary")]
public class DisciplinaryController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public DisciplinaryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public IActionResult GetAllCases(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });

    [HttpPost]
    [Authorize(Policy = "HrOnly")]
    public IActionResult CreateCase(CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
    
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "HrOnly")]
    public IActionResult GetCaseById(Guid id, CancellationToken ct) => Ok(new { Message = "Not implemented yet" });
}
