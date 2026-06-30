using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaxOmbud.Application.Auth.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record VerifyMfaRequest(string TotpCode);
public record DisableMfaRequest(string Password);

/// <summary>
/// Handles taxpayer self-registration, login and token refresh.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new taxpayer portal account.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [HttpPost("/api/modules/auth/signup")]
    [ProducesResponseType(typeof(Response<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Authenticate and receive JWT + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [HttpPost("/api/modules/auth/signin")]
    [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Exchange a refresh token for a new access token.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Response<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Logout the user by revoking their refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        var result = await _authService.LogoutAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Initiate password reset process by email.</summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Reset password using token received in email.</summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Change the current user's password.</summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.ChangePasswordAsync(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Verify email address using verification token.</summary>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        var result = await _authService.VerifyEmailAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Generate a new MFA TOTP secret and backup codes.</summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/setup")]
    [ProducesResponseType(typeof(Response<SetupMfaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetupMfa(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.SetupMfaAsync(new SetupMfaCommand(userId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Verify the TOTP code to complete MFA setup and enable it.</summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.VerifyMfaAsync(new VerifyMfaCommand(userId, request.TotpCode), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Disable MFA for the current user.</summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableMfa([FromBody] DisableMfaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.DisableMfaAsync(new DisableMfaCommand(userId, request.Password), ct);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
