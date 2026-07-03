namespace TaxOmbud.Application.Wallet.DTOs;

public record ProcessWithdrawalCommands(Guid TransactionId, bool Approved) ;
