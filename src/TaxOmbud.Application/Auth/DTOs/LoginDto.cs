using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Auth.DTOs;

// ─── Login ────────────────────────────────────────────────────────────────────
/// <summary>
/// Login request. UserType is required to route the login to the correct
/// portal (Taxpayer portal vs Staff/Admin dashboard) and validate identity.
/// </summary>
public record LoginCommand(
    string Email,
    string Password,
    UserType UserType = UserType.StaffUser,
    string? TotpCode = null
);

/// <summary>
/// Login success response. Includes UserType so the front-end can redirect
/// to the appropriate portal immediately after login.
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool MfaRequired,
    Guid UserId,
    string FullName,
    string UserType,
    string? Email,
    IReadOnlyList<string> Roles
);
