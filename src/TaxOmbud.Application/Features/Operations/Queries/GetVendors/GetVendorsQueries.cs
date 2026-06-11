using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetVendors;

public record GetVendorsQueries : IRequest<Result<GetVendorsResponse>>
{
}

public class GetVendorsResponse
{
    public bool Success { get; set; }
}

public class GetVendorsQueriesHandler : IRequestHandler<GetVendorsQueries, Result<GetVendorsResponse>>
{
    public async Task<Result<GetVendorsResponse>> Handle(GetVendorsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetVendorsResponse>.Success(new GetVendorsResponse { Success = true });
    }
}