using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Commands.WithdrawEwa;

// ─── Command ─────────────────────────────────────────────────────────────────

public record WithdrawEwaCommand(decimal Amount) : IRequest<Result<EwaWithdrawalResponse>>;

public record EwaWithdrawalResponse(string Message, decimal Amount);

// ─── Validator ────────────────────────────────────────────────────────────────

public class WithdrawEwaCommandValidator : AbstractValidator<WithdrawEwaCommand>
{
    public WithdrawEwaCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class WithdrawEwaCommandHandler : IRequestHandler<WithdrawEwaCommand, Result<EwaWithdrawalResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public WithdrawEwaCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<EwaWithdrawalResponse>> Handle(WithdrawEwaCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.UserId == currentUserId, cancellationToken);
        if (wallet == null || wallet.BalanceNgn < request.Amount)
        {
            return Result<EwaWithdrawalResponse>.Failure("Insufficient wallet balance.");
        }

        var req = new EwaRequest
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Amount = request.Amount,
            Status = "approved",
            DisbursedAt = DateTimeOffset.UtcNow
        };

        wallet.BalanceNgn -= request.Amount;
        wallet.Transactions.Add(new WalletTransaction
        {
            Id = Guid.NewGuid(), // Ensure WalletTransaction has an Id if it needs one, or let it auto-generate, let's look at controller: it didn't set Id so we set it just in case, or not. The original controller had: WalletId, Type, Amount, Reference. Let's see: in summary, "Entities like RolePermission and UserPermissionOverride use composite keys and do not have an Id property. Other entities do."
            WalletId = wallet.UserId,
            Type = "debit",
            Amount = request.Amount,
            Reference = "EWA-" + Guid.NewGuid().ToString("N").Substring(0, 8)
        });

        _context.EwaRequests.Add(req);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<EwaWithdrawalResponse>.Success(new EwaWithdrawalResponse("Earned Wage Access payout initiated successfully.", request.Amount));
    }
}
