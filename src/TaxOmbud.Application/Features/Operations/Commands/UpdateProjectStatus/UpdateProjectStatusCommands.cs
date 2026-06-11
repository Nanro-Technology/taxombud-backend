using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.UpdateProjectStatus;

public record UpdateProjectStatusCommands : IRequest<Result<UpdateProjectStatusResponse>>
{
}

public class UpdateProjectStatusResponse
{
    public bool Success { get; set; }
}

public class UpdateProjectStatusCommandsHandler : IRequestHandler<UpdateProjectStatusCommands, Result<UpdateProjectStatusResponse>>
{
    public async Task<Result<UpdateProjectStatusResponse>> Handle(UpdateProjectStatusCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<UpdateProjectStatusResponse>.Success(new UpdateProjectStatusResponse { Success = true });
    }
}