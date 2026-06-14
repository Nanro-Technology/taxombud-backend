using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Chats.DTOs;

namespace TaxOmbud.Application.Features.Chats.Commands;

public record MarkMessageAsReadCommand(Guid MessageId) : IRequest<bool>;

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public MarkMessageAsReadCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.AgentChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null) return false;

        var receipts = JsonSerializer.Deserialize<List<ReadReceiptDto>>(message.ReadReceipts) ?? new List<ReadReceiptDto>();
        
        if (!_currentUser.UserId.HasValue) return false;

        if (!receipts.Any(r => r.UserId == _currentUser.UserId.Value))
        {
            receipts.Add(new ReadReceiptDto
            {
                UserId = _currentUser.UserId.Value,
                ReadAt = DateTime.UtcNow
            });

            message.ReadReceipts = JsonSerializer.Serialize(receipts);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
