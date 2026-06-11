using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.RunPayroll;

public record RunPayrollCommands : IRequest<Result<RunPayrollResponse>>
{
}

public class RunPayrollResponse
{
    public bool Success { get; set; }
}

public class RunPayrollCommandsHandler : IRequestHandler<RunPayrollCommands, Result<RunPayrollResponse>>
{
    public async Task<Result<RunPayrollResponse>> Handle(RunPayrollCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<RunPayrollResponse>.Success(new RunPayrollResponse { Success = true });
    }
}
