using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.CreateProject;

public record CreateProjectCommands : IRequest<Result<CreateProjectResponse>>
{
}

public class CreateProjectResponse
{
    public bool Success { get; set; }
}

public class CreateProjectCommandsHandler : IRequestHandler<CreateProjectCommands, Result<CreateProjectResponse>>
{
    public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<CreateProjectResponse>.Success(new CreateProjectResponse { Success = true });
    }
}