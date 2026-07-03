using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Services;

public class AiChatbotService : IAiChatbotService
{

    public AiChatbotService(
    )
    {
    }

    public async Task<Response<SubmitChatMessageResponse>> SubmitChatMessageAsync(SubmitChatMessageCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SubmitChatMessageResponse>();
        try
        {
            var replyMessage = "I am an AI assistant for Tax Ombud. I have received your message: " + request.Message;
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "Message processed successfully.";
            response.Data = new SubmitChatMessageResponse(replyMessage);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while communicating with AI Chatbot.";
            return response;
        }
    }

}
