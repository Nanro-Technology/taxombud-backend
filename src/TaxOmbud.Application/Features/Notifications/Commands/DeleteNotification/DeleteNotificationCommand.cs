using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Notifications.Commands.DeleteNotification;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeleteNotificationCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteNotificationCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == userId, cancellationToken);

        if (notification == null)
            return Result<Unit>.NotFound("Notification not found.");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
