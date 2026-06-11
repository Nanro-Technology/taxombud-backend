using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetEwaRequests;

public record GetEwaRequestsQueries : IRequest<Result<GetEwaRequestsResponse>>
{
}

public class GetEwaRequestsResponse
{
    public bool Success { get; set; }
}

public class GetEwaRequestsQueriesHandler : IRequestHandler<GetEwaRequestsQueries, Result<GetEwaRequestsResponse>>
{
    public async Task<Result<GetEwaRequestsResponse>> Handle(GetEwaRequestsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetEwaRequestsResponse>.Success(new GetEwaRequestsResponse { Success = true });
    }
}
