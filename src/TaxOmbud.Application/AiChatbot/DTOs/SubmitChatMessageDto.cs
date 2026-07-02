namespace TaxOmbud.Application.AiChatbot.DTOs;

public record SubmitChatMessageCommand(
    string SessionId,
    string Message
) ;

public record SubmitChatMessageResponse(string Reply);
