using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Commands.RequestWithdrawal;

public record RequestWithdrawalCommands(Guid WalletId, decimal Amount) : IRequest<Result<Guid>>;

public class RequestWithdrawalCommandsHandler : IRequestHandler<RequestWithdrawalCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public RequestWithdrawalCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(RequestWithdrawalCommands request, CancellationToken cancellationToken)
    {
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(x => x.Id == request.WalletId, cancellationToken);
        if (wallet == null) return Result<Guid>.NotFound("Wallet not found.");
        if (wallet.BalanceNgn < request.Amount) return Result<Guid>.Failure("Insufficient balance.");
        
        var tx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = request.WalletId,
            Amount = -request.Amount,
            Type = "debit", Reference = "WithdrawalRequest",
            
            
        };
        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(tx.Id);
    }
}