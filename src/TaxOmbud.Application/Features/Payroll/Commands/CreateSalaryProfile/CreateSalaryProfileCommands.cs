using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.CreateSalaryProfile;

public record CreateSalaryProfileCommands : IRequest<Result<CreateSalaryProfileResponse>>
{
}

public class CreateSalaryProfileResponse
{
    public bool Success { get; set; }
}

public class CreateSalaryProfileCommandsHandler : IRequestHandler<CreateSalaryProfileCommands, Result<CreateSalaryProfileResponse>>
{
    public async Task<Result<CreateSalaryProfileResponse>> Handle(CreateSalaryProfileCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<CreateSalaryProfileResponse>.Success(new CreateSalaryProfileResponse { Success = true });
    }
}
