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

namespace TaxOmbud.Application.Features.Chats.Queries;

public record GetChatMessagesQuery(Guid ChatId) : IRequest<List<ChatMessageDto>>;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetChatMessagesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return new List<ChatMessageDto>();
        var userIdStr = _currentUser.UserId.Value.ToString();
        
        // Verify user is part of the chat
        var chat = await _context.AgentChats
            .FirstOrDefaultAsync(c => c.Id == request.ChatId && c.ParticipantIds.Contains(userIdStr), cancellationToken);
            
        if (chat == null) return new List<ChatMessageDto>();

        var messages = await _context.AgentChatMessages
            .Where(m => m.AgentChatId == request.ChatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatId = m.AgentChatId,
            SenderId = m.SenderId,
            Content = m.Content,
            IsPinned = m.IsPinned,
            AttachmentUrl = m.AttachmentUrl,
            AttachmentFileName = m.AttachmentFileName,
            CreatedAt = m.CreatedAt,
            ReadReceipts = JsonSerializer.Deserialize<List<ReadReceiptDto>>(m.ReadReceipts) ?? new List<ReadReceiptDto>()
        }).ToList();
    }
}
