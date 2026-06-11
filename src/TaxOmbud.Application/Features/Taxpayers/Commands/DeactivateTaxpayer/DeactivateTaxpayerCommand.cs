using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Taxpayers.Commands.DeactivateTaxpayer;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeactivateTaxpayerCommand(Guid TaxpayerId) : IRequest<Result<Unit>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeactivateTaxpayerCommandHandler : IRequestHandler<DeactivateTaxpayerCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateTaxpayerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeactivateTaxpayerCommand request, CancellationToken cancellationToken)
    {
        var taxpayer = await _context.TaxpayerProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);

        if (taxpayer is null)
            return Result<Unit>.NotFound("Taxpayer not found.");

        taxpayer.User.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
