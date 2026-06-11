using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Queries.GetWalletTransactions;

public record GetWalletTransactionsQueries : IRequest<Result<GetWalletTransactionsResponse>>
{
}

public class GetWalletTransactionsResponse
{
    public bool Success { get; set; }
}

public class GetWalletTransactionsQueriesHandler : IRequestHandler<GetWalletTransactionsQueries, Result<GetWalletTransactionsResponse>>
{
    public async Task<Result<GetWalletTransactionsResponse>> Handle(GetWalletTransactionsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetWalletTransactionsResponse>.Success(new GetWalletTransactionsResponse { Success = true });
    }
}
