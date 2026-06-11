using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Features.Notifications.Commands.SendNotification;

// ─── Command ─────────────────────────────────────────────────────────────────

public record SendNotificationCommand(Guid UserId, string Title, string Message) : IRequest<Result<SentNotificationResponse>>;

public record SentNotificationResponse(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<SentNotificationResponse>>
{
    private readonly IApplicationDbContext _context;

    public SendNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SentNotificationResponse>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<SentNotificationResponse>.Failure("Target user not found.");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new SentNotificationResponse(notification.Id, notification.Title, notification.CreatedAt);
        return Result<SentNotificationResponse>.Success(response);
    }
}
