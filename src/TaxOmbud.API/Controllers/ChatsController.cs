using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaxOmbud.API.Hubs;
using TaxOmbud.Application.Features.Chats.Commands;
using TaxOmbud.Application.Features.Chats.DTOs;
using TaxOmbud.Application.Features.Chats.Queries;

namespace TaxOmbud.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public ChatsController(IMediator mediator, IHubContext<ChatHub, IChatClient> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChatDto>>> GetChats()
    {
        var result = await _mediator.Send(new GetChatsQuery());
        return Ok(result);
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(Guid id)
    {
        var result = await _mediator.Send(new GetChatMessagesQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateChat([FromBody] CreateChatCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(Guid id, [FromForm] string content, [FromForm] IFormFile? attachment, [FromForm] string participantIdsJson = "[]")
    {
        string? attachmentUrl = null;
        string? attachmentFileName = null;

        if (attachment != null && attachment.Length > 0)
        {
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chats");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = $"{Guid.NewGuid()}_{attachment.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }

            attachmentUrl = $"/uploads/chats/{fileName}";
            attachmentFileName = attachment.FileName;
        }

        var command = new SendMessageCommand(id, content, attachmentUrl, attachmentFileName);
        var result = await _mediator.Send(command);

        if (result != null)
        {
            // Parse participantIds from form
            var participantIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(participantIdsJson) ?? new List<string>();
            foreach(var pId in participantIds)
            {
                await _hubContext.Clients.Group(pId).ReceiveMessage(result);
            }
            return Ok(result);
        }

        return BadRequest("Failed to send message.");
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<ActionResult> MarkMessageAsRead(Guid messageId, [FromBody] MarkReadRequest request)
    {
        var success = await _mediator.Send(new MarkMessageAsReadCommand(messageId));
        if (success)
        {
            foreach (var pId in request.ParticipantIds)
            {
                await _hubContext.Clients.Group(pId).MessageRead(messageId, request.UserId);
            }
            return Ok();
        }
        return BadRequest();
    }
    
    [HttpPost("messages/{messageId}/pin")]
    public async Task<ActionResult> PinMessage(Guid messageId, [FromBody] PinMessageRequest request)
    {
        var success = await _mediator.Send(new PinMessageCommand(messageId, request.IsPinned));
        if (success) return Ok();
        return BadRequest();
    }
}

public class MarkReadRequest
{
    public Guid UserId { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
}

public class PinMessageRequest
{
    public bool IsPinned { get; set; }
}
