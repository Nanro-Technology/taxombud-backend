using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Chats.DTOs;

public class ChatDto
{
    public Guid Id { get; set; }
    public string? Topic { get; set; }
    public bool IsGroupChat { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
    
    public ChatMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
