using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.ApprovePayroll;

public record ApprovePayrollCommands : IRequest<Result<ApprovePayrollResponse>>
{
}

public class ApprovePayrollResponse
{
    public bool Success { get; set; }
}

public class ApprovePayrollCommandsHandler : IRequestHandler<ApprovePayrollCommands, Result<ApprovePayrollResponse>>
{
    public async Task<Result<ApprovePayrollResponse>> Handle(ApprovePayrollCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<ApprovePayrollResponse>.Success(new ApprovePayrollResponse { Success = true });
    }
}
