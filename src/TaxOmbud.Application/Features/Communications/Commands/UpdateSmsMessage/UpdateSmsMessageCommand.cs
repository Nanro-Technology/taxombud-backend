using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.UpdateSmsMessage;

public record UpdateSmsMessageCommand : IRequest
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
}

public class UpdateSmsMessageCommandValidator : AbstractValidator<UpdateSmsMessageCommand>
{
    public UpdateSmsMessageCommandValidator()
    {
        RuleFor(v => v.Status).MaximumLength(50).NotEmpty();
    }
}

public class UpdateSmsMessageCommandHandler : IRequestHandler<UpdateSmsMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSmsMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateSmsMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
