using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Chats.DTOs;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Chats.Commands;

public record SendMessageCommand(Guid ChatId, string Content, string? AttachmentUrl, string? AttachmentFileName) : IRequest<ChatMessageDto?>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatMessageDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public SendMessageCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChatMessageDto?> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var chat = await _context.AgentChats
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat == null || !_currentUser.UserId.HasValue) return null;

        var message = new AgentChatMessage
        {
            AgentChatId = request.ChatId,
            SenderId = _currentUser.UserId.Value,
            Content = request.Content,
            AttachmentUrl = request.AttachmentUrl,
            AttachmentFileName = request.AttachmentFileName,
            ReadReceipts = "[]" // JSON string
        };

        _context.AgentChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChatMessageDto
        {
            Id = message.Id,
            ChatId = message.AgentChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            IsPinned = message.IsPinned,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentFileName = message.AttachmentFileName,
            CreatedAt = message.CreatedAt,
            ReadReceipts = new List<ReadReceiptDto>()
        };
    }
}
