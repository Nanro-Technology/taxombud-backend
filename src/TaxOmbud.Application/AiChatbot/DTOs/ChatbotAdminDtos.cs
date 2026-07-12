using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.AiChatbot.DTOs;

public record ChatbotSessionListDto(
    Guid Id,
    string VisitorName,
    string? VisitorEmail,
    string Platform,
    string Status,
    string Preview,
    DateTime CreatedAt,
    string? AssignedAgentId,
    string? AssignedAgentName
);

public record ChatbotMessageDto(
    string Sender,
    string Text,
    string Time,
    List<string>? Citations
);

public record ChatbotSessionDetailDto(
    Guid Id,
    string VisitorName,
    string? VisitorEmail,
    string Platform,
    string Status,
    string Preview,
    DateTime CreatedAt,
    string? AssignedAgentId,
    string? AssignedAgentName,
    List<ChatbotMessageDto> Messages
);

public record ChatbotStatsDto(
    int OpenSessions,
    int HandoffQueue,
    int MessagesToday,
    int AllMessages
);

public record ChatbotSettingDto(
    string BotName,
    string DefaultLanguage,
    string WelcomeMsg,
    string SystemPrompt,
    string FallbackMsg,
    string HandoffMsg,
    bool AutoOpen,
    int AutoOpenDelay,
    List<string> AllowedLanguages,
    List<StarterPromptDto> StarterPrompts,
    List<RAGSourceDto> RagSources
);

public record StarterPromptDto(
    string Title,
    string Subtitle,
    string Prompt
);

public record RAGSourceDto(
    string Source,
    string Status,
    int Chunks
);

public record UnansweredQuestionDto(
    string Question,
    int Hits,
    string LastSeen
);

public record UpdateChatbotStatusRequest(
    string Status
);

public record AgentReplyRequest(
    string Message
);
