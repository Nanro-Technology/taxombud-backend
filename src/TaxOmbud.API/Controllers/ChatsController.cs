using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TaxOmbud.API.Hubs;
using TaxOmbud.Application.Chats.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

public class MarkReadRequest
{
    public Guid UserId { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
}
public class PinMessageRequest
{
    public bool IsPinned { get; set; }
}

/// <summary>
/// Real-time messaging between staff and taxpayers, backed by SignalR.
/// </summary>
[ApiController]
[Route("api/v1/chats")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class ChatsController : ControllerBase
{
    private readonly IChatsService _chatsService;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public ChatsController(IChatsService chatsService, IHubContext<ChatHub, IChatClient> hubContext)
    {
        _chatsService = chatsService;
        _hubContext = hubContext;
    }

    /// <summary>Get all chats for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetChats(CancellationToken ct)
    {
        var result = await _chatsService.GetChatsAsync(new GetChatsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Get messages for a specific chat.</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, CancellationToken ct)
    {
        var result = await _chatsService.GetChatMessagesAsync(new GetChatMessagesQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Create a new chat.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatCommand command, CancellationToken ct)
    {
        var result = await _chatsService.CreateChatAsync(command, ct);
        return Ok(result);
    }

    /// <summary>Send a message in a chat, with optional file attachment.</summary>
    [HttpPost("{id:guid}/messages")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromForm] string content,
        [FromForm] IFormFile? attachment,
        [FromForm] string participantIdsJson = "[]",
        CancellationToken ct = default)
    {
        string? attachmentUrl = null;
        string? attachmentFileName = null;

        if (attachment != null && attachment.Length > 0)
        {
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chats");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            var fileName = $"{Guid.NewGuid()}_{attachment.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await attachment.CopyToAsync(stream);
            attachmentUrl = $"/uploads/chats/{fileName}";
            attachmentFileName = attachment.FileName;
        }

        var result = await _chatsService.SendMessageAsync(new SendMessageCommand(id, content, attachmentUrl, attachmentFileName), ct);
        if (result == null) return BadRequest("Failed to send message.");

        var participantIds = JsonSerializer.Deserialize<List<string>>(participantIdsJson) ?? new List<string>();
        foreach (var pId in participantIds)
            await _hubContext.Clients.Group(pId).ReceiveMessage(result);

        return Ok(result);
    }

    /// <summary>Mark a message as read.</summary>
    [HttpPost("messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId, [FromBody] MarkReadRequest request, CancellationToken ct)
    {
        var success = await _chatsService.MarkMessageAsReadAsync(new MarkMessageAsReadCommand(messageId), ct);
        if (!success) return BadRequest();
        foreach (var pId in request.ParticipantIds)
            await _hubContext.Clients.Group(pId).MessageRead(messageId, request.UserId);
        return Ok();
    }

    /// <summary>Pin or unpin a message.</summary>
    [HttpPost("messages/{messageId:guid}/pin")]
    public async Task<IActionResult> PinMessage(Guid messageId, [FromBody] PinMessageRequest request, CancellationToken ct)
    {
        var success = await _chatsService.PinMessageAsync(new PinMessageCommand(messageId, request.IsPinned), ct);
        if (success) return Ok();
        return BadRequest();
    }
}
