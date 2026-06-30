using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.AiChatbot.DTOs;

public record SubmitChatMessageCommand(
    string SessionId,
    string Message
) ;

public record SubmitChatMessageResponse(string Reply);
