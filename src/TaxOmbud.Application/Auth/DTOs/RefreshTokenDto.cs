namespace TaxOmbud.Application.Auth.DTOs;

public record RefreshTokenCommand(string Token) ;
public record RefreshTokenResponse(string AccessToken, string NewRefreshToken, DateTimeOffset ExpiresAt);