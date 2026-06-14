using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.SubmitLeaveRequest;

public record SubmitLeaveRequestCommands(Guid StaffId, string LeaveType, DateTime StartDate, DateTime EndDate, string Reason) : IRequest<Result<Guid>>;

public class SubmitLeaveRequestCommandsHandler : IRequestHandler<SubmitLeaveRequestCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public SubmitLeaveRequestCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(SubmitLeaveRequestCommands request, CancellationToken cancellationToken)
    {
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            UserId = request.StaffId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = (int)(request.EndDate - request.StartDate).TotalDays,
            Status = "Pending",
            
        };
        _context.LeaveRequests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}