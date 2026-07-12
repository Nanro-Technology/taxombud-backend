using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.AiChatbot.DTOs;

public record SubmitChatMessageCommand(
    string? SessionId,
    string Message
);

public record SubmitChatMessageResponse(
    Guid SessionId,
    string Reply,
    List<string>? Citations = null
);
