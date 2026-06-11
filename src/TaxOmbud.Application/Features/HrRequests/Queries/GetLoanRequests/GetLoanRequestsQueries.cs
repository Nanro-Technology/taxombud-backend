using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetLoanRequests;

public record GetLoanRequestsQueries : IRequest<Result<GetLoanRequestsResponse>>
{
}

public class GetLoanRequestsResponse
{
    public bool Success { get; set; }
}

public class GetLoanRequestsQueriesHandler : IRequestHandler<GetLoanRequestsQueries, Result<GetLoanRequestsResponse>>
{
    public async Task<Result<GetLoanRequestsResponse>> Handle(GetLoanRequestsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetLoanRequestsResponse>.Success(new GetLoanRequestsResponse { Success = true });
    }
}
