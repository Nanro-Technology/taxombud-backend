using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetProjects;

public record GetProjectsQueries : IRequest<Result<GetProjectsResponse>>
{
}

public class GetProjectsResponse
{
    public bool Success { get; set; }
}

public class GetProjectsQueriesHandler : IRequestHandler<GetProjectsQueries, Result<GetProjectsResponse>>
{
    public async Task<Result<GetProjectsResponse>> Handle(GetProjectsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetProjectsResponse>.Success(new GetProjectsResponse { Success = true });
    }
}