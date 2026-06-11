using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Commands.RequestWithdrawal;

public record RequestWithdrawalCommands : IRequest<Result<RequestWithdrawalResponse>>
{
}

public class RequestWithdrawalResponse
{
    public bool Success { get; set; }
}

public class RequestWithdrawalCommandsHandler : IRequestHandler<RequestWithdrawalCommands, Result<RequestWithdrawalResponse>>
{
    public async Task<Result<RequestWithdrawalResponse>> Handle(RequestWithdrawalCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<RequestWithdrawalResponse>.Success(new RequestWithdrawalResponse { Success = true });
    }
}
