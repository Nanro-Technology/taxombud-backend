using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAiChatbotService
{
    Task<Response<SubmitChatMessageResponse>> SubmitChatMessageAsync(SubmitChatMessageCommand request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<ChatbotSessionListDto>>> GetSessionsAsync(string? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Response<ChatbotSessionDetailDto>> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<object?>> SendAgentReplyAsync(Guid id, string message, string agentId, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateSessionStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteSessionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<ChatbotStatsDto>> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<Response<ChatbotSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateSettingsAsync(ChatbotSettingDto settings, CancellationToken cancellationToken = default);
    Task<Response<List<UnansweredQuestionDto>>> GetUnansweredQuestionsAsync(CancellationToken cancellationToken = default);
}
