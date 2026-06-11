using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetInventoryItems;

public record GetInventoryItemsQueries : IRequest<Result<GetInventoryItemsResponse>>
{
}

public class GetInventoryItemsResponse
{
    public bool Success { get; set; }
}

public class GetInventoryItemsQueriesHandler : IRequestHandler<GetInventoryItemsQueries, Result<GetInventoryItemsResponse>>
{
    public async Task<Result<GetInventoryItemsResponse>> Handle(GetInventoryItemsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetInventoryItemsResponse>.Success(new GetInventoryItemsResponse { Success = true });
    }
}