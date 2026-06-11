using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetQuotes;

public record GetQuotesQueries : IRequest<Result<GetQuotesResponse>>
{
}

public class GetQuotesResponse
{
    public bool Success { get; set; }
}

public class GetQuotesQueriesHandler : IRequestHandler<GetQuotesQueries, Result<GetQuotesResponse>>
{
    public async Task<Result<GetQuotesResponse>> Handle(GetQuotesQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetQuotesResponse>.Success(new GetQuotesResponse { Success = true });
    }
}