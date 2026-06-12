using System;

namespace TaxOmbud.Application.Features.Communications.DTOs;

public class AgentChatMessageDto
{
    public Guid Id { get; set; }
    public Guid AgentChatId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsPinned { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
