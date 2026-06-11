using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Commands.ProcessWithdrawal;

public record ProcessWithdrawalCommands : IRequest<Result<ProcessWithdrawalResponse>>
{
}

public class ProcessWithdrawalResponse
{
    public bool Success { get; set; }
}

public class ProcessWithdrawalCommandsHandler : IRequestHandler<ProcessWithdrawalCommands, Result<ProcessWithdrawalResponse>>
{
    public async Task<Result<ProcessWithdrawalResponse>> Handle(ProcessWithdrawalCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<ProcessWithdrawalResponse>.Success(new ProcessWithdrawalResponse { Success = true });
    }
}
