using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Auth.Commands.RefreshToken;

// ─── Command ─────────────────────────────────────────────────────────────────

public record RefreshTokenCommand(string Token) : IRequest<Result<RefreshTokenResponse>>;
public record RefreshTokenResponse(string AccessToken, string NewRefreshToken, DateTimeOffset ExpiresAt);

// ─── Validator ────────────────────────────────────────────────────────────────

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken);

        if (storedToken is null)
            return Result<RefreshTokenResponse>.Failure("Invalid refresh token.");

        if (storedToken.IsRevoked)
            return Result<RefreshTokenResponse>.Failure("Refresh token has been revoked.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return Result<RefreshTokenResponse>.Failure("Refresh token has expired. Please log in again.");

        var user = storedToken.User;

        var roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Rotate: revoke old, issue new
        storedToken.Revoke();

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles, permissions);
        var (newRefreshToken, expiry) = _tokenService.GenerateRefreshToken();

        var newRt = new TaxOmbud.Domain.Entities.Identity.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = expiry,
            CreatedAt = DateTimeOffset.UtcNow,
            ReplacedByToken = null
        };
        storedToken.ReplacedByToken = newRefreshToken;

        _context.RefreshTokens.Add(newRt);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(accessToken, newRefreshToken, expiry));
    }
}
