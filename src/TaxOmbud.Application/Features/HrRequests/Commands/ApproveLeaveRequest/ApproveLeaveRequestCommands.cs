using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.ApproveLeaveRequest;

public record ApproveLeaveRequestCommands(Guid LeaveId, bool Approved) : IRequest<Result<bool>>;

public class ApproveLeaveRequestCommandsHandler : IRequestHandler<ApproveLeaveRequestCommands, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ApproveLeaveRequestCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ApproveLeaveRequestCommands request, CancellationToken cancellationToken)
    {
        var entity = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (entity == null) return Result<bool>.NotFound($"Leave Request {request.LeaveId} not found.");
        
        entity.Status = request.Approved ? "Approved" : "Rejected";
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}