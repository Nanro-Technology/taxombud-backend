using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaxOmbud.Application.Chats.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Services;

public class ChatsService : IChatsService
{
    private readonly IGenericRepository<AgentChat> _chatRepo;
    private readonly IGenericRepository<AgentChatMessage> _messageRepo;
    private readonly ICurrentUser _currentUser;

    public ChatsService(
        IGenericRepository<AgentChat> chatRepo,
        IGenericRepository<AgentChatMessage> messageRepo,
        ICurrentUser currentUser
    )
    {
        _chatRepo = chatRepo;
        _messageRepo = messageRepo;
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

        await _chatRepo.AddAsync(chat);
        await _chatRepo.SaveAsync();

        return chat.Id;
    }

    public async Task<bool> MarkMessageAsReadAsync(MarkMessageAsReadCommand request, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepo.FindAsync(m => m.Id == request.MessageId);

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
            await _messageRepo.UpdateAsync(message);
            await _messageRepo.SaveAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> PinMessageAsync(PinMessageCommand request, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepo.FindAsync(m => m.Id == request.MessageId);

        if (message == null) return false;

        message.IsPinned = request.IsPinned;
        await _messageRepo.UpdateAsync(message);
        await _messageRepo.SaveAsync();

        return true;
    }

    public async Task<ChatMessageDto?> SendMessageAsync(SendMessageCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepo.FindAsync(c => c.Id == request.ChatId);

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

        await _messageRepo.AddAsync(message);
        await _messageRepo.SaveAsync();

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
        var chat = await _chatRepo.Query()
            .FirstOrDefaultAsync(c => c.Id == request.ChatId && c.ParticipantIds.Contains(userIdStr), cancellationToken);
            
        if (chat == null) return new List<ChatMessageDto>();

        var messages = await _messageRepo.Query()
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

        var chats = await _chatRepo.Query()
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
                
                unreadCount = await _messageRepo.Query()
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

        return result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTimeOffset.MinValue).ToList();
    }

    public async Task<bool> MarkChatAsReadAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return false;
        var userId = _currentUser.UserId.Value;
        var userIdStr = userId.ToString();

        var unreadMessages = await _messageRepo.Query()
            .Where(m => m.AgentChatId == chatId && m.SenderId != userId && !m.ReadReceipts.Contains(userIdStr))
            .ToListAsync(cancellationToken);

        if (!unreadMessages.Any()) return true;

        foreach (var message in unreadMessages)
        {
            var receipts = JsonSerializer.Deserialize<List<ReadReceiptDto>>(message.ReadReceipts) ?? new List<ReadReceiptDto>();
            if (!receipts.Any(r => r.UserId == userId))
            {
                receipts.Add(new ReadReceiptDto
                {
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
                message.ReadReceipts = JsonSerializer.Serialize(receipts);
                await _messageRepo.UpdateAsync(message);
            }
        }

        await _messageRepo.SaveAsync();
        return true;
    }
}
