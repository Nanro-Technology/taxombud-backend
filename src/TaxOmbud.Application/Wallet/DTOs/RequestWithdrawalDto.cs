namespace TaxOmbud.Application.Wallet.DTOs;

public record RequestWithdrawalCommands(Guid WalletId, decimal Amount) ;
