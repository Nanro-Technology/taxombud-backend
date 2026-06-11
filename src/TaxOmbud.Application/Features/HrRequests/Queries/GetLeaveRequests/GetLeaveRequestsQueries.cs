using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetLeaveRequests;

public record GetLeaveRequestsQueries : IRequest<Result<GetLeaveRequestsResponse>>
{
}

public class GetLeaveRequestsResponse
{
    public bool Success { get; set; }
}

public class GetLeaveRequestsQueriesHandler : IRequestHandler<GetLeaveRequestsQueries, Result<GetLeaveRequestsResponse>>
{
    public async Task<Result<GetLeaveRequestsResponse>> Handle(GetLeaveRequestsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetLeaveRequestsResponse>.Success(new GetLeaveRequestsResponse { Success = true });
    }
}
