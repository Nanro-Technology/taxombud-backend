using TaxOmbud.Application.Auth.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAuthService
{
    // ─── Taxpayer self-service registration (public portal) ───────────────────
    Task<Response<RegisterResponse>> RegisterTaxpayerAsync(RegisterTaxpayerCommand request, CancellationToken cancellationToken = default);

    // ─── Staff registration (admin-only) ──────────────────────────────────────
    Task<Response<RegisterResponse>> RegisterAsync(RegisterCommand request, CancellationToken cancellationToken = default);

    // ─── Auth flows ───────────────────────────────────────────────────────────
    Task<Response<LoginResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken = default);
    Task<Response<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenCommand request, CancellationToken cancellationToken = default);

    // ─── Password management ──────────────────────────────────────────────────
    Task<Response<object?>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ForgotPasswordAsync(ForgotPasswordCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken = default);

    // ─── Email verification ───────────────────────────────────────────────────
    Task<Response<object?>> VerifyEmailAsync(VerifyEmailCommand request, CancellationToken cancellationToken = default);

    // ─── MFA ──────────────────────────────────────────────────────────────────
    Task<Response<SetupMfaResponse>> SetupMfaAsync(SetupMfaCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> VerifyMfaAsync(VerifyMfaCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DisableMfaAsync(DisableMfaCommand request, CancellationToken cancellationToken = default);
}
