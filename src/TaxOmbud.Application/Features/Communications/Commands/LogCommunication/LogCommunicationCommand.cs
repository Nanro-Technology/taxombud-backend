using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Communications.Commands.LogCommunication;

// ─── Command ─────────────────────────────────────────────────────────────────

public record LogCommunicationCommand(
    string Channel,
    string Subject,
    string Body,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType
) : IRequest<Result<LoggedCommunicationResponse>>;

public record LoggedCommunicationResponse(
    Guid Id,
    string Channel,
    string Subject,
    string Recipient,
    bool IsSent,
    DateTimeOffset? SentAt
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class LogCommunicationCommandValidator : AbstractValidator<LogCommunicationCommand>
{
    public LogCommunicationCommandValidator()
    {
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Recipient).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientName).MaximumLength(200);
        RuleFor(x => x.RelatedEntityType).MaximumLength(100);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class LogCommunicationCommandHandler : IRequestHandler<LogCommunicationCommand, Result<LoggedCommunicationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public LogCommunicationCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LoggedCommunicationResponse>> Handle(LogCommunicationCommand request, CancellationToken cancellationToken)
    {
        var actorUserId = _currentUser.UserId ?? Guid.Empty;

        var log = new CommunicationLog
        {
            Id = Guid.NewGuid(),
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            Recipient = request.Recipient,
            RecipientName = request.RecipientName,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            Direction = CommunicationDirection.Outbound,
            IsSent = true,
            SentAt = DateTimeOffset.UtcNow,
            SentByUserId = actorUserId
        };

        _context.CommunicationLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoggedCommunicationResponse(log.Id, log.Channel, log.Subject, log.Recipient, log.IsSent, log.SentAt);
        return Result<LoggedCommunicationResponse>.Success(response);
    }
}
