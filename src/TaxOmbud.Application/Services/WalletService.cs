using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Wallet.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class WalletService : IWalletService
{
    private readonly IGenericRepository<EmployeeWallet> _walletRepo;
    private readonly IGenericRepository<WalletTransaction> _txRepo;

    public WalletService(
        IGenericRepository<EmployeeWallet> walletRepo,
        IGenericRepository<WalletTransaction> txRepo)
    {
        _walletRepo = walletRepo;
        _txRepo = txRepo;
    }

    public async Task<Response<bool>> ProcessWithdrawalAsync(ProcessWithdrawalCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var tx = await _txRepo.FindAsync(x => x.Id == request.TransactionId);
            if (tx == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Transaction not found.";
                response.Data = false;
                return response;
            }

            if (request.Approved)
            {
                var wallet = await _walletRepo.GetByIdAsync(tx.WalletId);
                if (wallet != null)
                {
                    wallet.BalanceNgn += tx.Amount; // amount is already negative
                    await _walletRepo.UpdateAsync(wallet);
                }
            }

            await _txRepo.SaveAsync();

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
            var wallet = await _walletRepo.FindAsync(x => x.Id == request.WalletId);
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
                CreatedAt = DateTime.UtcNow
            };

            await _txRepo.AddAsync(tx);
            await _txRepo.SaveAsync();

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
            var wallet = await _walletRepo.FindAsync(x => x.UserId == request.UserId);
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
            var txs = await _txRepo.FindAllAsync(x => x.WalletId == request.WalletId);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Transactions retrieved successfully.";
            response.Data = txs.ToList();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving transactions.";
        }
        return response;
    }
}
