using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.AddInventoryItem;

public record AddInventoryItemCommands : IRequest<Result<AddInventoryItemResponse>>
{
}

public class AddInventoryItemResponse
{
    public bool Success { get; set; }
}

public class AddInventoryItemCommandsHandler : IRequestHandler<AddInventoryItemCommands, Result<AddInventoryItemResponse>>
{
    public async Task<Result<AddInventoryItemResponse>> Handle(AddInventoryItemCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<AddInventoryItemResponse>.Success(new AddInventoryItemResponse { Success = true });
    }
}