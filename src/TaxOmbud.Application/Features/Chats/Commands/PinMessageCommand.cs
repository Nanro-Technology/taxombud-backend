using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Application.Features.Chats.Commands;

public record PinMessageCommand(Guid MessageId, bool IsPinned) : IRequest<bool>;

public class PinMessageCommandHandler : IRequestHandler<PinMessageCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public PinMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(PinMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.AgentChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null) return false;

        message.IsPinned = request.IsPinned;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
