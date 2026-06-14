using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.CreateSmsMessage;

public record CreateSmsMessageCommand : IRequest<Guid>
{
    public string Provider { get; set; } = null!;
    public string? SenderId { get; set; }
    public string Body { get; set; } = null!;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string RecipientType { get; set; } = null!;
    public string? PhoneNumbers { get; set; }
    public string Mode { get; set; } = null!;
    public string Direction { get; set; } = null!;
}

public class CreateSmsMessageCommandValidator : AbstractValidator<CreateSmsMessageCommand>
{
    public CreateSmsMessageCommandValidator()
    {
        RuleFor(v => v.Provider).MaximumLength(100).NotEmpty();
        RuleFor(v => v.SenderId).MaximumLength(100);
        RuleFor(v => v.Body).NotEmpty();
        RuleFor(v => v.RecipientType).MaximumLength(50).NotEmpty();
        RuleFor(v => v.Mode).MaximumLength(50).NotEmpty();
        RuleFor(v => v.Direction).MaximumLength(50).NotEmpty();
    }
}

public class CreateSmsMessageCommandHandler : IRequestHandler<CreateSmsMessageCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSmsMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateSmsMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = new SmsMessage
        {
            Provider = request.Provider,
            SenderId = request.SenderId,
            Body = request.Body,
            ScheduledAt = request.ScheduledAt,
            RecipientType = request.RecipientType,
            PhoneNumbers = request.PhoneNumbers,
            Mode = request.Mode,
            Direction = request.Direction,
            Status = "Pending"
        };

        _context.SmsMessages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
