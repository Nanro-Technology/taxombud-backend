using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Commands.ProcessWithdrawal;

public record ProcessWithdrawalCommands(Guid TransactionId, bool Approved) : IRequest<Result<bool>>;

public class ProcessWithdrawalCommandsHandler : IRequestHandler<ProcessWithdrawalCommands, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ProcessWithdrawalCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ProcessWithdrawalCommands request, CancellationToken cancellationToken)
    {
        var tx = await _context.WalletTransactions.FirstOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);
        if (tx == null) return Result<bool>.NotFound("Transaction not found.");

        if (request.Approved)
        {
            
            var wallet = await _context.EmployeeWallets.FindAsync(new object[] { tx.WalletId }, cancellationToken);
            if(wallet != null) 
            {
                wallet.BalanceNgn += tx.Amount; // amount is already negative
            }
        }
        else
        {
            
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}