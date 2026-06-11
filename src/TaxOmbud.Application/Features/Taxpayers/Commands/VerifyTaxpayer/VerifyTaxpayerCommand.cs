using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Taxpayers.Commands.VerifyTaxpayer;

// ─── Command ─────────────────────────────────────────────────────────────────

public record VerifyTaxpayerCommand(Guid TaxpayerId, bool IsVerified) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class VerifyTaxpayerCommandValidator : AbstractValidator<VerifyTaxpayerCommand>
{
    public VerifyTaxpayerCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class VerifyTaxpayerCommandHandler : IRequestHandler<VerifyTaxpayerCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public VerifyTaxpayerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(VerifyTaxpayerCommand request, CancellationToken cancellationToken)
    {
        var taxpayer = await _context.TaxpayerProfiles.FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);
        if (taxpayer == null)
            return Result<Unit>.NotFound("Taxpayer profile not found.");

        taxpayer.IsVerified = request.IsVerified;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
