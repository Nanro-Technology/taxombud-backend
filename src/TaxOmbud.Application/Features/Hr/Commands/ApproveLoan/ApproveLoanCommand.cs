using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Hr.Commands.ApproveLoan;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ApproveLoanCommand(Guid Id, bool Approved) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ApproveLoanCommandValidator : AbstractValidator<ApproveLoanCommand>
{
    public ApproveLoanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ApproveLoanCommandHandler : IRequestHandler<ApproveLoanCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ApproveLoanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await _context.LoanRequests.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (loan == null)
            return Result<Unit>.NotFound("Loan request not found.");

        loan.Status = request.Approved ? "approved" : "rejected";
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
