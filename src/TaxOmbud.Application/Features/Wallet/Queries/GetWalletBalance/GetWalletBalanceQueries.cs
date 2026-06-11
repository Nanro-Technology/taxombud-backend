using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Queries.GetWalletBalance;

public record GetWalletBalanceQueries : IRequest<Result<GetWalletBalanceResponse>>
{
}

public class GetWalletBalanceResponse
{
    public bool Success { get; set; }
}

public class GetWalletBalanceQueriesHandler : IRequestHandler<GetWalletBalanceQueries, Result<GetWalletBalanceResponse>>
{
    public async Task<Result<GetWalletBalanceResponse>> Handle(GetWalletBalanceQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetWalletBalanceResponse>.Success(new GetWalletBalanceResponse { Success = true });
    }
}
