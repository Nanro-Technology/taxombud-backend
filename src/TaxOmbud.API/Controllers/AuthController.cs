using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Auth.Commands.ChangePassword;
using TaxOmbud.Application.Features.Auth.Commands.DisableMfa;
using TaxOmbud.Application.Features.Auth.Commands.ForgotPassword;
using TaxOmbud.Application.Features.Auth.Commands.Login;
using TaxOmbud.Application.Features.Auth.Commands.Logout;
using TaxOmbud.Application.Features.Auth.Commands.RefreshToken;
using TaxOmbud.Application.Features.Auth.Commands.Register;
using TaxOmbud.Application.Features.Auth.Commands.ResetPassword;
using TaxOmbud.Application.Features.Auth.Commands.SetupMfa;
using TaxOmbud.Application.Features.Auth.Commands.VerifyEmail;
using TaxOmbud.Application.Features.Auth.Commands.VerifyMfa;

namespace TaxOmbud.Api.Controllers;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record VerifyMfaRequest(string TotpCode);
public record DisableMfaRequest(string Password);

/// <summary>
/// Handles taxpayer self-registration, login and token refresh.
/// </summary>
[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Register a new taxpayer portal account.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>Authenticate and receive JWT + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Exchange a refresh token for a new access token.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Logout the user by revoking their refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Initiate password reset process by email.</summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Reset password using token received in email.</summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Change the current user's password.</summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Verify email address using verification token.</summary>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Generate a new MFA TOTP secret and backup codes.</summary>
    [Authorize]
    [HttpPost("mfa/setup")]
    [ProducesResponseType(typeof(SetupMfaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetupMfa(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new SetupMfaCommand(userId), ct);
        return ToActionResult(result);
    }

    /// <summary>Verify the TOTP code to complete MFA setup and enable it.</summary>
    [Authorize]
    [HttpPost("mfa/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new VerifyMfaCommand(userId, request.TotpCode);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Disable MFA for the current user.</summary>
    [Authorize]
    [HttpPost("mfa/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisableMfa([FromBody] DisableMfaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new DisableMfaCommand(userId, request.Password);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
