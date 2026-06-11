using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetSalaryProfiles;

public record GetSalaryProfilesQueries : IRequest<Result<GetSalaryProfilesResponse>>
{
}

public class GetSalaryProfilesResponse
{
    public bool Success { get; set; }
}

public class GetSalaryProfilesQueriesHandler : IRequestHandler<GetSalaryProfilesQueries, Result<GetSalaryProfilesResponse>>
{
    public async Task<Result<GetSalaryProfilesResponse>> Handle(GetSalaryProfilesQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetSalaryProfilesResponse>.Success(new GetSalaryProfilesResponse { Success = true });
    }
}
