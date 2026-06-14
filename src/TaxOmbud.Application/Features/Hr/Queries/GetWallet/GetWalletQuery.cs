using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Queries.GetWallet;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetWalletQuery() : IRequest<Result<WalletDto>>;

public record WalletDto(
    Guid UserId,
    decimal BalanceNgn,
    int LedgerVersion,
    IEnumerable<WalletTransactionDto> Transactions
);

public record WalletTransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    string Reference,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetWalletQueryHandler : IRequestHandler<GetWalletQuery, Result<WalletDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetWalletQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<WalletDto>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        var wallet = await _context.EmployeeWallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == currentUserId, cancellationToken);

        if (wallet == null)
        {
            wallet = new EmployeeWallet { UserId = currentUserId, BalanceNgn = 0, LedgerVersion = 1 };
            _context.EmployeeWallets.Add(wallet);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = new WalletDto(
            wallet.UserId,
            wallet.BalanceNgn,
            wallet.LedgerVersion,
            wallet.Transactions.Select(t => new WalletTransactionDto(
                t.Id,
                t.Type,
                t.Amount,
                t.Reference,
                t.CreatedAt
            ))
        );

        await Task.CompletedTask; return Result<WalletDto>.Success(dto);
    }
}
