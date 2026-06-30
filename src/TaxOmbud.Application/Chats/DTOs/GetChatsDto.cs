using System;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Chats.DTOs;

public record GetChatsQuery;
public record GetChatMessagesQuery(Guid ChatId);
public record CreateChatCommand(string Topic, bool IsGroupChat, List<Guid> ParticipantIds);
public record SendMessageCommand(Guid ChatId, string Content, string? AttachmentUrl, string? AttachmentFileName);
public record MarkMessageAsReadCommand(Guid MessageId);
public record PinMessageCommand(Guid MessageId, bool IsPinned);

public class ReadReceiptDto
{
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<ReadReceiptDto> ReadReceipts { get; set; } = new();
}

public class ChatDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public bool IsGroupChat { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
    public ChatMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}