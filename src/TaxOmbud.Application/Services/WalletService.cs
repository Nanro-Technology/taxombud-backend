using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Wallet.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class WalletService : IWalletService
{
    private readonly IApplicationDbContext _context;

    public WalletService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<bool>> ProcessWithdrawalAsync(ProcessWithdrawalCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var tx = await _context.WalletTransactions
                .FirstOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);

            if (tx == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Transaction not found.";
                response.Data = false;
                return response;
            }

            if (request.Approved)
            {
                var wallet = await _context.EmployeeWallets.FindAsync(new object[] { tx.WalletId }, cancellationToken);
                if (wallet != null)
                {
                    wallet.BalanceNgn += tx.Amount; // amount is already negative
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Withdrawal processed successfully.";
            response.Data = true;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while processing the withdrawal.";
            response.Data = false;
        }
        return response;
    }

    public async Task<Response<Guid>> RequestWithdrawalAsync(RequestWithdrawalCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var wallet = await _context.EmployeeWallets
                .FirstOrDefaultAsync(x => x.Id == request.WalletId, cancellationToken);

            if (wallet == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Wallet not found.";
                return response;
            }

            if (wallet.BalanceNgn < request.Amount)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Insufficient balance.";
                return response;
            }

            var tx = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = request.WalletId,
                Amount = -request.Amount,
                Type = "debit",
                Reference = "WithdrawalRequest",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.WalletTransactions.Add(tx);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Withdrawal request submitted successfully.";
            response.Data = tx.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while requesting withdrawal.";
        }
        return response;
    }

    public async Task<Response<EmployeeWallet>> GetWalletBalanceAsync(GetWalletBalanceQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<EmployeeWallet>();
        try
        {
            var wallet = await _context.EmployeeWallets
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (wallet == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Wallet not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Wallet balance retrieved successfully.";
            response.Data = wallet;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving wallet balance.";
        }
        return response;
    }

    public async Task<Response<List<WalletTransaction>>> GetWalletTransactionsAsync(GetWalletTransactionsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<WalletTransaction>>();
        try
        {
            var txs = await _context.WalletTransactions
                .Where(x => x.WalletId == request.WalletId)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Transactions retrieved successfully.";
            response.Data = txs;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving transactions.";
        }
        return response;
    }
}