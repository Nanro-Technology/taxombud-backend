using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.IdentityVerification.Commands.VerifyIdentity;

namespace TaxOmbud.Api.Controllers;

[AllowAnonymous]
[Route("api/public")]
public class IdentityVerificationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public IdentityVerificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("identity-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyIdentity([FromBody] VerifyIdentityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}
