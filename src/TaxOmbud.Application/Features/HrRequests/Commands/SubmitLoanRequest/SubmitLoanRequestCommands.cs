using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.SubmitLoanRequest;

public record SubmitLoanRequestCommands : IRequest<Result<SubmitLoanRequestResponse>>
{
}

public class SubmitLoanRequestResponse
{
    public bool Success { get; set; }
}

public class SubmitLoanRequestCommandsHandler : IRequestHandler<SubmitLoanRequestCommands, Result<SubmitLoanRequestResponse>>
{
    public async Task<Result<SubmitLoanRequestResponse>> Handle(SubmitLoanRequestCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<SubmitLoanRequestResponse>.Success(new SubmitLoanRequestResponse { Success = true });
    }
}
