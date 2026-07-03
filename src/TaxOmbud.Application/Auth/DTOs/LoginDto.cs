namespace TaxOmbud.Application.Auth.DTOs;

public record LoginCommand(
    string Email,
    string Password,
    string? TotpCode = null
) ;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool MfaRequired,
    Guid UserId,
    string FullName,
    IReadOnlyList<string> Roles
);
