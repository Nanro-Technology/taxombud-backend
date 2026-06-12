using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Chats.DTOs;

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

public class ReadReceiptDto
{
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; }
}
