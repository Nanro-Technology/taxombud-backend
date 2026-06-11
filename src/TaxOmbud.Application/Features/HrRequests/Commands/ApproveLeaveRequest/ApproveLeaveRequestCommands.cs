using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.ApproveLeaveRequest;

public record ApproveLeaveRequestCommands : IRequest<Result<ApproveLeaveRequestResponse>>
{
}

public class ApproveLeaveRequestResponse
{
    public bool Success { get; set; }
}

public class ApproveLeaveRequestCommandsHandler : IRequestHandler<ApproveLeaveRequestCommands, Result<ApproveLeaveRequestResponse>>
{
    public async Task<Result<ApproveLeaveRequestResponse>> Handle(ApproveLeaveRequestCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<ApproveLeaveRequestResponse>.Success(new ApproveLeaveRequestResponse { Success = true });
    }
}
