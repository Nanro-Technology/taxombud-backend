using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.SubmitLeaveRequest;

public record SubmitLeaveRequestCommands : IRequest<Result<SubmitLeaveRequestResponse>>
{
}

public class SubmitLeaveRequestResponse
{
    public bool Success { get; set; }
}

public class SubmitLeaveRequestCommandsHandler : IRequestHandler<SubmitLeaveRequestCommands, Result<SubmitLeaveRequestResponse>>
{
    public async Task<Result<SubmitLeaveRequestResponse>> Handle(SubmitLeaveRequestCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<SubmitLeaveRequestResponse>.Success(new SubmitLeaveRequestResponse { Success = true });
    }
}
