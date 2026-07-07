using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaxOmbud.Application.Auth.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Handles authentication, user registration, token management, and Multi-Factor Authentication (MFA) operations.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new taxpayer portal account (Public Self-Registration).
    /// </summary>
    /// <remarks>
    /// This endpoint is used by public taxpayers to register their portal accounts. 
    /// The user is created as a <c>RegisteredTaxpayer</c> user type and does not receive a system role.
    /// </remarks>
    /// <param name="command">The taxpayer registration details, including alt profile metadata (gender, NIN, address, city, state, country).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="201">Returns the registration response containing the created user ID and details.</response>
    /// <response code="400">If validation fails or an account with the email already exists.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(Response<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterTaxpayerCommand command, CancellationToken ct)
    {
        var result = await _authService.RegisterTaxpayerAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Registers a new staff user account (Admin only).
    /// </summary>
    /// <remarks>
    /// This endpoint requires authentication. It is used by SuperAdmin or Admin accounts to create new system staff members.
    /// You must specify a valid role ID to assign to the staff member (e.g. Officer, Manager).
    /// </remarks>
    /// <param name="command">The registration details of the staff member.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="201">Returns the registration response containing the created user ID and details.</response>
    /// <response code="400">If the request is invalid or the target role does not exist.</response>
    /// <response code="401">If the requester is not authenticated.</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("register-staff")]
    [ProducesResponseType(typeof(Response<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStaff([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Authenticates a user and issues JWT access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts login credentials. You must provide the correct <c>userType</c> for the portal you are trying to access:
    /// - <c>2</c> (RegisteredTaxpayer) for the public taxpayer portal.
    /// - <c>3</c> (StaffUser) for the internal staff dashboards.
    /// </remarks>
    /// <param name="command">The login credentials and target user portal classification.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">Returns access token, refresh token, expiration time, and role assignments.</response>
    /// <response code="400">If credentials or userType matching validation fails.</response>
    /// <response code="403">If the account is disabled or lacks login permissions.</response>
    /// <response code="423">If the user is currently locked out due to too many failed attempts.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Rotates and exchanges a refresh token for a new JWT access token.
    /// </summary>
    /// <remarks>
    /// Performs token rotation, invalidating the old refresh token and providing a fresh access and refresh token pair.
    /// </remarks>
    /// <param name="command">The valid refresh token command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">Returns the new token pair.</response>
    /// <response code="400">If the token is expired, revoked, or invalid.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Response<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Logs out the user by revoking the provided refresh token.
    /// </summary>
    /// <param name="command">The refresh token to revoke.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If log out and token revocation succeeded.</response>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        var result = await _authService.LogoutAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Initiates the password recovery flow (sends reset email).
    /// </summary>
    /// <remarks>
    /// Always returns OK (200) to prevent email harvesting and verification probing.
    /// </remarks>
    /// <param name="command">The email address of the account to recover.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">Indicates the recovery process was initiated successfully.</response>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Resets the user password using a token received in email.
    /// </summary>
    /// <param name="command">The password reset token, email address, and new password details.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If the password reset was successful.</response>
    /// <response code="400">If the token is invalid or has expired.</response>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="request">The current password and new password details.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If the password was changed successfully.</response>
    /// <response code="400">If the current password is incorrect or new password validation fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.ChangePasswordAsync(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Verifies the user email address using verification token.
    /// </summary>
    /// <param name="command">The verification token received.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If verification succeeded.</response>
    /// <response code="400">If the token is invalid or expired.</response>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        var result = await _authService.VerifyEmailAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Sets up Multi-Factor Authentication (MFA) for the current user.
    /// </summary>
    /// <remarks>
    /// Generates a new TOTP secret key, a QR Code URI for authenticator applications, and 8 secure backup codes.
    /// </remarks>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">Returns the MFA setup details including secret and backup codes.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/setup")]
    [ProducesResponseType(typeof(Response<SetupMfaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetupMfa(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.SetupMfaAsync(new SetupMfaCommand(userId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Verifies the TOTP code to complete MFA activation and enable it on the account.
    /// </summary>
    /// <param name="request">The TOTP code generated by the authenticator application.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If the code is verified and MFA is enabled.</response>
    /// <response code="400">If the code is invalid or has expired.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/verify")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _authService.VerifyMfaAsync(new VerifyMfaCommand(userId, request.TotpCode), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Disables Multi-Factor Authentication (MFA) for the currently authenticated user.
    /// </summary>
    /// <param name="request">The password verification block to authorize the action.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <response code="200">If MFA is successfully disabled.</response>
    /// <response code="400">If the password confirmation fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/disable")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status400BadRequest)]
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
