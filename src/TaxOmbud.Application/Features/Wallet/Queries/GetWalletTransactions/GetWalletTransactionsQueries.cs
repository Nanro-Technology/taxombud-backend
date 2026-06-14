using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Queries.GetWalletTransactions;

public record GetWalletTransactionsQueries(Guid WalletId) : IRequest<Result<List<WalletTransaction>>>;

public class GetWalletTransactionsQueriesHandler : IRequestHandler<GetWalletTransactionsQueries, Result<List<WalletTransaction>>>
{
    private readonly IApplicationDbContext _context;
    public GetWalletTransactionsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<WalletTransaction>>> Handle(GetWalletTransactionsQueries request, CancellationToken cancellationToken)
    {
        var txs = await _context.WalletTransactions
            .Where(x => x.WalletId == request.WalletId)
            .ToListAsync(cancellationToken);
        return Result<List<WalletTransaction>>.Success(txs);
    }
}