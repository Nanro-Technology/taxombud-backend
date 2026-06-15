using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.AiChatbot.Commands.SubmitChatMessage;

public record SubmitChatMessageCommand(
    string SessionId,
    string Message
) : IRequest<Result<SubmitChatMessageResponse>>;

public record SubmitChatMessageResponse(string Reply);

public class SubmitChatMessageCommandHandler : IRequestHandler<SubmitChatMessageCommand, Result<SubmitChatMessageResponse>>
{
    public Task<Result<SubmitChatMessageResponse>> Handle(SubmitChatMessageCommand request, CancellationToken cancellationToken)
    {
        // Mocked response for AI Chatbot
        var replyMessage = "I am an AI assistant for Tax Ombud. I have received your message: " + request.Message;
        return Task.FromResult(Result<SubmitChatMessageResponse>.Success(new SubmitChatMessageResponse(replyMessage)));
    }
}
