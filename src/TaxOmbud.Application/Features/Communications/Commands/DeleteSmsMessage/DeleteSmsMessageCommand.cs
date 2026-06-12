using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.DeleteSmsMessage;

public record DeleteSmsMessageCommand(Guid Id) : IRequest;

public class DeleteSmsMessageCommandHandler : IRequestHandler<DeleteSmsMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSmsMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSmsMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        _context.SmsMessages.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
