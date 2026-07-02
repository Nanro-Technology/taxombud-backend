using System.Security.Claims;

namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface ITokenService
{
    /// <summary>Generates a short-lived JWT access token.</summary>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>Generates a cryptographically-random refresh token and records its expiry.</summary>
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();

    /// <summary>Validates a JWT access token and returns the principal.</summary>
    ClaimsPrincipal? ValidateAccessToken(string token);
}
