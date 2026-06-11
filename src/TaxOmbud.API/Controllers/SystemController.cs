using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.SystemSettings.Commands.ToggleE2ee;
using TaxOmbud.Application.Features.SystemSettings.Queries.GetE2eeStatus;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// System configuration and settings management.
/// </summary>
[Authorize]
[Route("api/v1/system")]
public class SystemController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SystemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get current E2EE status.</summary>
    [AllowAnonymous]
    [HttpGet("settings/e2ee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetE2eeStatus(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetE2eeStatusQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Toggle global E2EE status.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("settings/e2ee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleE2ee([FromBody] ToggleE2eeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleE2eeCommand(request.Enable), ct);
        return ToActionResult(result);
    }
}

public record ToggleE2eeRequest(bool Enable);
