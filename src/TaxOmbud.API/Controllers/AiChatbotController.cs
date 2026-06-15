using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.AiChatbot.Commands.SubmitChatMessage;

namespace TaxOmbud.Api.Controllers;

[AllowAnonymous]
[Route("api/public")]
public class AiChatbotController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AiChatbotController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ai-chatbot/chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] SubmitChatMessageCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}
