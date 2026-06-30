using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class AiChatbotController : ControllerBase
{
    private readonly IAiChatbotService _aiChatbotService;

    public AiChatbotController(IAiChatbotService aiChatbotService)
    {
        _aiChatbotService = aiChatbotService;
    }

    [HttpPost("ai-chatbot/chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] SubmitChatMessageCommand command, CancellationToken ct)
    {
        var result = await _aiChatbotService.SubmitChatMessageAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
