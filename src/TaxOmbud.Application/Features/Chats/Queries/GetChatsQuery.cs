using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Chats.DTOs;
using System.Text.Json;
using System;

namespace TaxOmbud.Application.Features.Chats.Queries;

public record GetChatsQuery : IRequest<List<ChatDto>>;

public class GetChatsQueryHandler : IRequestHandler<GetChatsQuery, List<ChatDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetChatsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ChatDto>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return new List<ChatDto>();
        var userIdStr = _currentUser.UserId.Value.ToString();
        if (string.IsNullOrEmpty(userIdStr)) return new List<ChatDto>();

        var chats = await _context.AgentChats
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => c.ParticipantIds.Contains(userIdStr))
            .ToListAsync(cancellationToken);

        var result = new List<ChatDto>();
        foreach (var chat in chats)
        {
            var pIds = JsonSerializer.Deserialize<List<string>>(chat.ParticipantIds) ?? new List<string>();
            var pGuidIds = pIds.Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToList();
            
            var lastMsg = chat.Messages.FirstOrDefault();
            ChatMessageDto? lastMsgDto = null;
            int unreadCount = 0;
            
            if (lastMsg != null)
            {
                var receipts = JsonSerializer.Deserialize<List<ReadReceiptDto>>(lastMsg.ReadReceipts) ?? new List<ReadReceiptDto>();
                lastMsgDto = new ChatMessageDto
                {
                    Id = lastMsg.Id,
                    ChatId = lastMsg.AgentChatId,
                    SenderId = lastMsg.SenderId,
                    Content = lastMsg.Content,
                    IsPinned = lastMsg.IsPinned,
                    AttachmentUrl = lastMsg.AttachmentUrl,
                    AttachmentFileName = lastMsg.AttachmentFileName,
                    CreatedAt = lastMsg.CreatedAt,
                    ReadReceipts = receipts
                };
                
                unreadCount = await _context.AgentChatMessages
                    .Where(m => m.AgentChatId == chat.Id && m.SenderId != _currentUser.UserId.Value)
                    .Where(m => !m.ReadReceipts.Contains(userIdStr))
                    .CountAsync(cancellationToken);
            }

            result.Add(new ChatDto
            {
                Id = chat.Id,
                Topic = chat.Topic,
                IsGroupChat = chat.IsGroupChat,
                ParticipantIds = pGuidIds,
                LastMessage = lastMsgDto,
                UnreadCount = unreadCount
            });
        }

        return result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue).ToList();
    }
}
