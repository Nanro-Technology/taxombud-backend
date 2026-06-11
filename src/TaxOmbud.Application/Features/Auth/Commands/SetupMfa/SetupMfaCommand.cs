using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Auth.Commands.SetupMfa;

// ─── Command ─────────────────────────────────────────────────────────────────

public record SetupMfaCommand(Guid UserId) : IRequest<Result<SetupMfaResponse>>;

public record SetupMfaResponse(
    string QrCodeUri,
    string SecretKey,
    IReadOnlyList<string> BackupCodes
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class SetupMfaCommandHandler : IRequestHandler<SetupMfaCommand, Result<SetupMfaResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public SetupMfaCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<SetupMfaResponse>> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.MfaToken)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result<SetupMfaResponse>.NotFound("User not found.");

        // Generate a new TOTP secret (160-bit = 20 bytes per RFC 6238)
        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        var secretBase32 = Base32Encoding.ToString(secretBytes);

        // Generate 8 backup codes
        var backupCodes = Enumerable.Range(0, 8)
            .Select(_ => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant())
            .ToList();

        // Hash backup codes for storage
        var hashedBackups = string.Join(",", backupCodes.Select(c => _passwordHasher.Hash(c)));

        if (user.MfaToken is null)
        {
            var mfaToken = new MfaToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = secretBase32,
                IsEnabled = false,
                BackupCodesHash = hashedBackups
            };
            _context.MfaTokens.Add(mfaToken);
        }
        else
        {
            user.MfaToken.SecretKey = secretBase32;
            user.MfaToken.IsEnabled = false;
            user.MfaToken.BackupCodesHash = hashedBackups;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var label = Uri.EscapeDataString(user.Email);
        var issuer = Uri.EscapeDataString("TaxOmbud");
        var qrUri = $"otpauth://totp/{issuer}:{label}?secret={secretBase32}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

        return Result<SetupMfaResponse>.Success(new SetupMfaResponse(
            QrCodeUri: qrUri,
            SecretKey: secretBase32,
            BackupCodes: backupCodes.AsReadOnly()
        ));
    }
}
