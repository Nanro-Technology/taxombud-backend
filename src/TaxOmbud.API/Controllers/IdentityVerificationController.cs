using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.IdentityVerification.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class IdentityVerificationController : ControllerBase
{
    private readonly IIdentityVerificationService _identityVerificationService;

    public IdentityVerificationController(IIdentityVerificationService identityVerificationService)
    {
        _identityVerificationService = identityVerificationService;
    }

    [HttpPost("identity-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyIdentity([FromBody] VerifyIdentityCommand command, CancellationToken ct)
    {
        var result = await _identityVerificationService.VerifyIdentityAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
