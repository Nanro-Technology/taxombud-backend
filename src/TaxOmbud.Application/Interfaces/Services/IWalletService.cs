using TaxOmbud.Application.Wallet.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IWalletService
{
    Task<Response<bool>> ProcessWithdrawalAsync(ProcessWithdrawalCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> RequestWithdrawalAsync(RequestWithdrawalCommands request, CancellationToken cancellationToken = default);
    Task<Response<EmployeeWallet>> GetWalletBalanceAsync(GetWalletBalanceQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<WalletTransaction>>> GetWalletTransactionsAsync(GetWalletTransactionsQueries request, CancellationToken cancellationToken = default);
}
