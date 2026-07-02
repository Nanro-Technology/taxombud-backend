using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAiChatbotService
{
    Task<Response<SubmitChatMessageResponse>> SubmitChatMessageAsync(SubmitChatMessageCommand request, CancellationToken cancellationToken = default);
}
