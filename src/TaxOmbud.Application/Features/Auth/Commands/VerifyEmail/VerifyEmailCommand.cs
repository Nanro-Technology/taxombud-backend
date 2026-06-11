using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.VerifyEmail;

// ─── Command ─────────────────────────────────────────────────────────────────

public record VerifyEmailCommand(string Token) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, cancellationToken);

        if (user is null)
            return Result<object?>.Failure("Invalid or expired verification token.");

        if (user.EmailVerificationTokenExpiresAt < DateTimeOffset.UtcNow)
            return Result<object?>.Failure("Verification token has expired. Please request a new one.");

        if (user.EmailVerified)
            return Result<object?>.Success(null); // idempotent

        user.MarkEmailVerified();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
