using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Chats.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;
using System.Text.Json;

namespace TaxOmbud.Application.Services;

public class ChatsService : IChatsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ChatsService(
        IApplicationDbContext context,
        ICurrentUser currentUser
    )
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> CreateChatAsync(CreateChatCommand request, CancellationToken cancellationToken = default)
{
        var participants = new List<string>();
        if (_currentUser.UserId.HasValue && _currentUser.UserId.Value != Guid.Empty)
        {
            participants.Add(_currentUser.UserId.Value.ToString());
        }
        foreach(var id in request.ParticipantIds)
        {
            var s = id.ToString();
            if (!participants.Contains(s)) participants.Add(s);
        }

        var chat = new AgentChat
        {
            Topic = request.Topic,
            IsGroupChat = request.IsGroupChat,
            ParticipantIds = JsonSerializer.Serialize(participants)
        };

        _context.AgentChats.Add(chat);
        await _context.SaveChangesAsync(cancellationToken);

        return chat.Id;
    }

    public async Task<bool> MarkMessageAsReadAsync(MarkMessageAsReadCommand request, CancellationToken cancellationToken = default)
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

    public async Task<bool> PinMessageAsync(PinMessageCommand request, CancellationToken cancellationToken = default)
{
        var message = await _context.AgentChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null) return false;

        message.IsPinned = request.IsPinned;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ChatMessageDto?> SendMessageAsync(SendMessageCommand request, CancellationToken cancellationToken = default)
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

    public async Task<List<ChatMessageDto>> GetChatMessagesAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default)
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

    public async Task<List<ChatDto>> GetChatsAsync(GetChatsQuery request, CancellationToken cancellationToken = default)
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
                Topic = chat.Topic ?? string.Empty,
                IsGroupChat = chat.IsGroupChat,
                ParticipantIds = pGuidIds,
                LastMessage = lastMsgDto,
                UnreadCount = unreadCount
            });
        }

        return result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue).ToList();
    }

}