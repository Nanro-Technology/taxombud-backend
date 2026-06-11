using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetRemittances;

public record GetRemittancesQueries : IRequest<Result<GetRemittancesResponse>>
{
}

public class GetRemittancesResponse
{
    public bool Success { get; set; }
}

public class GetRemittancesQueriesHandler : IRequestHandler<GetRemittancesQueries, Result<GetRemittancesResponse>>
{
    public async Task<Result<GetRemittancesResponse>> Handle(GetRemittancesQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetRemittancesResponse>.Success(new GetRemittancesResponse { Success = true });
    }
}
