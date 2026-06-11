using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.VerifyMfa;

// ─── Command ─────────────────────────────────────────────────────────────────

public record VerifyMfaCommand(Guid UserId, string TotpCode) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.TotpCode).NotEmpty().Length(6).Matches(@"^\d{6}$");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public VerifyMfaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var mfaToken = await _context.MfaTokens
            .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);

        if (mfaToken is null)
            return Result<object?>.Failure("MFA has not been set up. Please call /mfa/setup first.");

        var secretBytes = Base32Encoding.ToBytes(mfaToken.SecretKey);
        var totp = new Totp(secretBytes);

        var isValid = totp.VerifyTotp(
            request.TotpCode,
            out _,
            new VerificationWindow(previous: 1, future: 1));

        if (!isValid)
            return Result<object?>.Failure("Invalid TOTP code.");

        mfaToken.IsEnabled = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
