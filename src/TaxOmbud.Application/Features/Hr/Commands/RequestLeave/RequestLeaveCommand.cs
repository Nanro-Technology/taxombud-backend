using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Commands.RequestLeave;

// ─── Command ─────────────────────────────────────────────────────────────────

public record RequestLeaveCommand(string LeaveType, DateTimeOffset StartDate, DateTimeOffset EndDate) : IRequest<Result<LeaveRequest>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class RequestLeaveCommandValidator : AbstractValidator<RequestLeaveCommand>
{
    public RequestLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class RequestLeaveCommandHandler : IRequestHandler<RequestLeaveCommand, Result<LeaveRequest>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public RequestLeaveCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LeaveRequest>> Handle(RequestLeaveCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        var leave = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = (request.EndDate - request.StartDate).Days + 1,
            Status = "pending"
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<LeaveRequest>.Success(leave);
    }
}
