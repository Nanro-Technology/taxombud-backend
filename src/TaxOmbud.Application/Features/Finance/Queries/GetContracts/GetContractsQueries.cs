using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetContracts;

public record GetContractsQueries : IRequest<Result<GetContractsResponse>>
{
}

public class GetContractsResponse
{
    public bool Success { get; set; }
}

public class GetContractsQueriesHandler : IRequestHandler<GetContractsQueries, Result<GetContractsResponse>>
{
    public async Task<Result<GetContractsResponse>> Handle(GetContractsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetContractsResponse>.Success(new GetContractsResponse { Success = true });
    }
}