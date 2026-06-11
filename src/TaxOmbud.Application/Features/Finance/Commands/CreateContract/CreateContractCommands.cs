using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.CreateContract;

public record CreateContractCommands : IRequest<Result<CreateContractResponse>>
{
}

public class CreateContractResponse
{
    public bool Success { get; set; }
}

public class CreateContractCommandsHandler : IRequestHandler<CreateContractCommands, Result<CreateContractResponse>>
{
    public async Task<Result<CreateContractResponse>> Handle(CreateContractCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<CreateContractResponse>.Success(new CreateContractResponse { Success = true });
    }
}