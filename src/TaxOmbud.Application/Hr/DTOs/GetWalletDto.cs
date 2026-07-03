using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record GetWalletQuery() ;

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
