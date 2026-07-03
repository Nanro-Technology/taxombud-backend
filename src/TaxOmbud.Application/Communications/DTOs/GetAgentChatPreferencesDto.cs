using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Communications.DTOs;

// ─── Agent Chat DTOs ───────────────────────────────────────────────────────────
public class AgentChatPreferenceDto
{
    public Guid UserId { get; set; }
    public bool DoNotDisturb { get; set; }
    public bool MarkAsAway { get; set; }
    public bool PlayNotificationSound { get; set; }
    public bool ShowBrowserNotifications { get; set; }
}

public class AgentSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public class AgentChatDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public bool IsGroupChat { get; set; }
    public List<AgentSummaryDto> Participants { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class AgentChatMessageDto
{
    public Guid Id { get; set; }
    public Guid AgentChatId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

// ─── SMS DTOs ─────────────────────────────────────────────────────────────────
public class SmsMessageDto
{
    public Guid Id { get; set; }
    public string? Provider { get; set; }
    public string? SenderId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? RecipientType { get; set; }
    public string? PhoneNumbers { get; set; }
    public string? Mode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Direction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

// ─── Agent Chat Queries / Commands ────────────────────────────────────────────
public record GetAgentChatPreferencesQuery;
public record GetAgentChatsQuery;
public record GetChatMessagesQuery(Guid ChatId);
public record SendAgentChatMessageCommand(Guid ChatId, string Content, string? AttachmentUrl, string? AttachmentFileName);
public record UpdateAgentChatPreferencesCommand(bool DoNotDisturb, bool MarkAsAway, bool PlayNotificationSound, bool ShowBrowserNotifications);
public record CreateAgentChatCommand(string Topic, bool IsGroupChat, List<Guid> ParticipantIds);

// ─── SMS Queries / Commands ───────────────────────────────────────────────────
public record GetSmsMessagesQuery;
public record GetSmsMessageByIdQuery(Guid Id);
public record CreateSmsMessageCommand(string Body, string? Provider, string? SenderId,
    DateTimeOffset? ScheduledAt, string? RecipientType, string? PhoneNumbers, string? Mode);
public record UpdateSmsMessageCommand(Guid Id, string Status);
public record DeleteSmsMessageCommand(Guid Id);

// ─── Agent Search Query ───────────────────────────────────────────────────────
public record SearchAgentsQuery(string? SearchTerm);
