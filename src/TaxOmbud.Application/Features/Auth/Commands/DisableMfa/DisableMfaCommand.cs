using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.DisableMfa;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DisableMfaCommand(Guid UserId, string Password) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DisableMfaCommandHandler : IRequestHandler<DisableMfaCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DisableMfaCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<object?>> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.MfaToken)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result<object?>.NotFound("User not found.");

        // Require password confirmation before disabling MFA
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<object?>.Failure("Password confirmation failed.");

        if (user.MfaToken is null || !user.MfaToken.IsEnabled)
            return Result<object?>.Success(null); // idempotent

        user.MfaToken.IsEnabled = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
