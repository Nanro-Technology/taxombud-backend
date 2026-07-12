using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AiChatbotController : ControllerBase
{
    private readonly IAiChatbotService _aiChatbotService;
    private readonly ICurrentUser _currentUser;

    public AiChatbotController(IAiChatbotService aiChatbotService, ICurrentUser currentUser)
    {
        _aiChatbotService = aiChatbotService;
        _currentUser = currentUser;
    }

    /// <summary>Submit a message to the public AI chatbot.</summary>
    [HttpPost("api/v1/public/ai-chatbot/chat")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Response<SubmitChatMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] SubmitChatMessageCommand command, CancellationToken ct)
    {
        var result = await _aiChatbotService.SubmitChatMessageAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get paginated and filtered list of chatbot sessions.</summary>
    [HttpGet("api/v1/ai-chatbot/sessions")]
    [ProducesResponseType(typeof(Response<PagedResult<ChatbotSessionListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _aiChatbotService.GetSessionsAsync(status, search, page, pageSize, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get chatbot session details and messages stream.</summary>
    [HttpGet("api/v1/ai-chatbot/sessions/{id:guid}")]
    [ProducesResponseType(typeof(Response<ChatbotSessionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(Guid id, CancellationToken ct)
    {
        var result = await _aiChatbotService.GetSessionByIdAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Send an agent reply to override the AI chatbot conversation.</summary>
    [HttpPost("api/v1/ai-chatbot/sessions/{id:guid}/reply")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendAgentReply(Guid id, [FromBody] AgentReplyRequest request, CancellationToken ct)
    {
        var agentId = _currentUser.UserId?.ToString() ?? Guid.Empty.ToString();
        var result = await _aiChatbotService.SendAgentReplyAsync(id, request.Message, agentId, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update session status (open, handoff, closed).</summary>
    [HttpPatch("api/v1/ai-chatbot/sessions/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSessionStatus(Guid id, [FromBody] UpdateChatbotStatusRequest request, CancellationToken ct)
    {
        var result = await _aiChatbotService.UpdateSessionStatusAsync(id, request.Status, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a chatbot conversation log.</summary>
    [HttpDelete("api/v1/ai-chatbot/sessions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(Guid id, CancellationToken ct)
    {
        var result = await _aiChatbotService.DeleteSessionAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get AI chatbot statistics for dashboard counters.</summary>
    [HttpGet("api/v1/ai-chatbot/stats")]
    [ProducesResponseType(typeof(Response<ChatbotStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _aiChatbotService.GetStatsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get chatbot settings configuration.</summary>
    [HttpGet("api/v1/ai-chatbot/settings")]
    [ProducesResponseType(typeof(Response<ChatbotSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _aiChatbotService.GetSettingsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Save or update chatbot settings configuration.</summary>
    [HttpPut("api/v1/ai-chatbot/settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSettings([FromBody] ChatbotSettingDto settings, CancellationToken ct)
    {
        var result = await _aiChatbotService.UpdateSettingsAsync(settings, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get chatbot unanswered questions list.</summary>
    [HttpGet("api/v1/ai-chatbot/unanswered")]
    [ProducesResponseType(typeof(Response<List<UnansweredQuestionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnansweredQuestions(CancellationToken ct)
    {
        var result = await _aiChatbotService.GetUnansweredQuestionsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }
}
