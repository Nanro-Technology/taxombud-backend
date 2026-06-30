using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage user mailbox, inbox, sent items, drafts, and trash.
/// </summary>
[Authorize]
[Route("api/v1/mailbox")]
public class MailboxController : ControllerBase
{
    private readonly ICommunicationsService _communicationsService;

    public MailboxController(
        ICommunicationsService communicationsService
    )
    {
        _communicationsService = communicationsService;
    }

    /// <summary>Send a new message or save as draft.</summary>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SendMessage([FromBody] SendMailboxMessageRequest request, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Mark a message as read.</summary>
    [HttpPut("{id:guid}/read")]
    public IActionResult MarkAsRead(Guid id, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Move a message to a different folder.</summary>
    [HttpPut("{id:guid}/move")]
    public IActionResult MoveMessage(Guid id, [FromBody] MoveMailboxMessageRequest request, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Delete a message permanently.</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteMessage(Guid id, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }
}

public record SendMailboxMessageRequest(string Subject, string BodyText, bool IsDraft, System.Collections.Generic.List<Guid> ToRecipients);
public record MoveMailboxMessageRequest(string Folder);

