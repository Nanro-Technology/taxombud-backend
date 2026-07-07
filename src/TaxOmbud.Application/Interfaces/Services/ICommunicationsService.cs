using TaxOmbud.Application.Communications.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ICommunicationsService
{
    Task<Response<object?>> AcknowledgeCommunicationAsync(AcknowledgeCommunicationCommand request, CancellationToken cancellationToken = default);
    Task<Guid> CreateAgentChatAsync(CreateAgentChatCommand request, CancellationToken cancellationToken = default);
    Task<Guid> CreateSmsMessageAsync(CreateSmsMessageCommand request, CancellationToken cancellationToken = default);
    Task<DeleteSmsMessageCommand> DeleteSmsMessageAsync(DeleteSmsMessageCommand request, CancellationToken cancellationToken = default);
    Task<Response<LoggedCommunicationResponse>> LogCommunicationAsync(LogCommunicationCommand request, CancellationToken cancellationToken = default);
    Task<Response<RenderedTemplateDto>> RenderCommunicationTemplateAsync(RenderCommunicationTemplateCommand request, CancellationToken cancellationToken = default);
    Task<Guid> SendAgentChatMessageAsync(SendAgentChatMessageCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> SendCommunicationAsync(SendCommunicationCommand request, CancellationToken cancellationToken = default);
    Task<object?> UpdateAgentChatPreferencesAsync(UpdateAgentChatPreferencesCommand request, CancellationToken cancellationToken = default);
    Task<UpdateSmsMessageCommand> UpdateSmsMessageAsync(UpdateSmsMessageCommand request, CancellationToken cancellationToken = default);
    Task<AgentChatPreferenceDto> GetAgentChatPreferencesAsync(GetAgentChatPreferencesQuery request, CancellationToken cancellationToken = default);
    Task<List<AgentChatDto>> GetAgentChatsAsync(GetAgentChatsQuery request, CancellationToken cancellationToken = default);
    Task<List<AgentChatMessageDto>> GetChatMessagesAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default);
    Task<Response<CommunicationDetailDto>> GetCommunicationByIdAsync(GetCommunicationByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<CommunicationListDto>>> GetCommunicationsAsync(GetCommunicationsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<CommunicationTemplateDto>>> GetCommunicationTemplatesAsync(GetCommunicationTemplatesQuery request, CancellationToken cancellationToken = default);
    Task<SmsMessageDto> GetSmsMessageByIdAsync(GetSmsMessageByIdQuery request, CancellationToken cancellationToken = default);
    Task<List<SmsMessageDto>> GetSmsMessagesAsync(GetSmsMessagesQuery request, CancellationToken cancellationToken = default);
    Task<List<AgentSummaryDto>> SearchAgentsAsync(SearchAgentsQuery request, CancellationToken cancellationToken = default);
}
