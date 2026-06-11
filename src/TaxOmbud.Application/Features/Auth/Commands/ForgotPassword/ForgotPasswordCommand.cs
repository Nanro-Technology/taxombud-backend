using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.ForgotPassword;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ForgotPasswordCommand(string Email) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<object?>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user is null)
            return Result<object?>.Success(null);

        // Generate a secure random reset token
        var token = Convert.ToBase64String(global::System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        user.SetPasswordResetToken(token);
        await _context.SaveChangesAsync(cancellationToken);

        // Send email with reset link
        await _emailService.SendAsync(
            to: user.Email,
            subject: "Reset your TaxOmbud password",
            htmlBody: $"Use this token to reset your password: {token}\n\nThis link expires in 1 hour.",
            cancellationToken: cancellationToken);

        return Result<object?>.Success(null);
    }
}
