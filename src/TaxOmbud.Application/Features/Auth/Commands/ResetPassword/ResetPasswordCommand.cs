using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.ResetPassword;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<object?>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
            return Result<object?>.Failure("Invalid or expired reset token.");

        if (user.PasswordResetToken != request.Token ||
            user.PasswordResetTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result<object?>.Failure("Invalid or expired reset token.");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.SetPasswordHash(newHash);
        user.ClearPasswordResetToken();

        // Revoke all existing refresh tokens for security
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.RefreshTokens.RemoveRange(tokens);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
