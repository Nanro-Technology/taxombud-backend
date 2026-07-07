using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Infrastructure.Options;

namespace TaxOmbud.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(_options.PrivateKeyPem);
        _privateKey = new RsaSecurityKey(privateRsa);

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(_options.PublicKeyPem);
        _publicKey = new RsaSecurityKey(publicRsa);
    }

    public string GenerateAccessToken(Guid userId, string email, UserType userType, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("user_type", userType.ToString()),
            new("usertype", userType.ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var signingCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiry = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays);
        return (token, expiry);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = _publicKey,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
