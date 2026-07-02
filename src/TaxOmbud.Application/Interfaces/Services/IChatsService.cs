using TaxOmbud.Application.Chats.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IChatsService
{
    Task<Guid> CreateChatAsync(CreateChatCommand request, CancellationToken cancellationToken = default);
    Task<bool> MarkMessageAsReadAsync(MarkMessageAsReadCommand request, CancellationToken cancellationToken = default);
    Task<bool> PinMessageAsync(PinMessageCommand request, CancellationToken cancellationToken = default);
    Task<ChatMessageDto?> SendMessageAsync(SendMessageCommand request, CancellationToken cancellationToken = default);
    Task<List<ChatMessageDto>> GetChatMessagesAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default);
    Task<List<ChatDto>> GetChatsAsync(GetChatsQuery request, CancellationToken cancellationToken = default);
}
