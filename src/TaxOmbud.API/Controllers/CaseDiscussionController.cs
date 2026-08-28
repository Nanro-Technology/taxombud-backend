using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TaxOmbud.API.Hubs;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Stage 5 Investigation Discussion Thread.
/// Provides endpoints to retrieve, post to, and manage the collaborative investigation thread
/// that is opened automatically when a case enters Stage 5.
/// </summary>
[ApiController]
[Route("api/v1/cases/{caseId}/discussion")]
[Tags("Stage 5 - Investigation Discussion")]
[Authorize]
public class CaseDiscussionController : ControllerBase
{
    private readonly ICaseDiscussionService _discussionService;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public CaseDiscussionController(
        ICaseDiscussionService discussionService,
        IHubContext<ChatHub, IChatClient> hubContext)
    {
        _discussionService = discussionService;
        _hubContext = hubContext;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // ─── Thread ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full investigation discussion thread for a case (messages + participants).
    /// Available to all participants (Tier 1, Tier 2, and Stage 6 read-only).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Response<CaseDiscussionThreadDto>>> GetThread(Guid caseId)
    {
        var thread = await _discussionService.GetDiscussionThreadAsync(caseId, GetUserId(), HttpContext.RequestAborted);
        if (thread == null)
            return NotFound(new Response<CaseDiscussionThreadDto> { StatusCode = 404, Message = "No investigation discussion thread found for this case." });

        return Ok(new Response<CaseDiscussionThreadDto> { StatusCode = 200, Message = "Thread retrieved.", Data = thread });
    }

    /// <summary>
    /// Manually open (or re-seed) the discussion thread for a case.
    /// Normally called automatically when the case enters Stage 5 — use this only for recovery/manual seeding.
    /// </summary>
    [HttpPost("open")]
    [Authorize]
    public async Task<ActionResult<Response<Guid>>> OpenThread(Guid caseId)
    {
        try
        {
            var chatId = await _discussionService.OpenDiscussionThreadAsync(caseId, HttpContext.RequestAborted);
            return Ok(new Response<Guid> { StatusCode = 200, Message = "Discussion thread opened.", Data = chatId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new Response<Guid> { StatusCode = 400, Message = ex.Message });
        }
    }

    /// <summary>
    /// Lock the discussion thread (typically called automatically when case advances to Stage 6).
    /// After locking, no new messages can be posted. Stage 6 officers gain read-only access.
    /// </summary>
    [HttpPost("lock")]
    [Authorize]
    public async Task<ActionResult<Response<bool>>> LockThread(Guid caseId)
    {
        var result = await _discussionService.LockDiscussionThreadAsync(caseId, HttpContext.RequestAborted);
        if (result)
        {
            await _hubContext.Clients.Group(ChatHub.GetCaseDiscussionGroupName(caseId))
                .DiscussionThreadLocked(caseId);
        }
        return Ok(new Response<bool> { StatusCode = 200, Message = "Discussion thread locked.", Data = result });
    }

    // ─── Messaging ────────────────────────────────────────────────────────────

    /// <summary>
    /// Post a message to the investigation discussion thread.
    /// MessageType options: "message" (default) | "finding" (formal finding) | "conclusion" (triggers supervisor notification).
    /// </summary>
    [HttpPost("messages")]
    public async Task<ActionResult<Response<DiscussionMessageDto>>> SendMessage(Guid caseId, [FromBody] SendDiscussionMessageRequest request)
    {
        var content = request.ResolvedContent;
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new Response<DiscussionMessageDto>
            {
                StatusCode = 400,
                Message = "Message content cannot be empty."
            });
        }

        var cmd = new SendDiscussionMessageCommand(
            caseId,
            content,
            request.MessageType ?? "message",
            request.AttachmentUrl,
            request.AttachmentFileName
        );

        try
        {
            var message = await _discussionService.SendMessageAsync(
                cmd,
                GetUserId(),
                HttpContext.RequestAborted);

            if (message == null)
                return BadRequest(new Response<DiscussionMessageDto>
                {
                    StatusCode = 400,
                    Message = "Message could not be sent. The thread may be locked or you lack permissions."
                });

            // Broadcast real-time discussion message to all room participants
            await _hubContext.Clients.Group(ChatHub.GetCaseDiscussionGroupName(caseId))
                .ReceiveDiscussionMessage(message);

            return Ok(new Response<DiscussionMessageDto> { StatusCode = 200, Message = "Message posted.", Data = message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new Response<DiscussionMessageDto>
            {
                StatusCode = 403,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new Response<DiscussionMessageDto>
            {
                StatusCode = 400,
                Message = ex.Message
            });
        }
    }

    /// <summary>Pin or unpin a message in the discussion thread. Admin/SuperAdmin only.</summary>
    [HttpPatch("messages/{messageId}/pin")]
    [Authorize]
    public async Task<ActionResult<Response<bool>>> PinMessage(Guid caseId, Guid messageId, [FromBody] PinDiscussionMessageRequest request)
    {
        var cmd = new PinDiscussionMessageCommand(messageId, request.IsPinned);
        var result = await _discussionService.PinMessageAsync(cmd, GetUserId(), HttpContext.RequestAborted);
        return Ok(new Response<bool> { StatusCode = 200, Message = request.IsPinned ? "Message pinned." : "Message unpinned.", Data = result });
    }

    // ─── Participant Management ───────────────────────────────────────────────

    /// <summary>
    /// Tier 2 override - Add all members of a role to this case's discussion thread.
    /// Admin/SuperAdmin only.
    /// </summary>
    [HttpPost("participants/roles")]
    [Authorize]
    public async Task<ActionResult<Response<bool>>> AddRoleToThread(Guid caseId, [FromBody] AddRoleToThreadRequest request)
    {
        var cmd = new AddRoleToDiscussionCommand(caseId, request.RoleName);
        var result = await _discussionService.AddRoleToThreadAsync(cmd, GetUserId(), HttpContext.RequestAborted);
        return Ok(new Response<bool> { StatusCode = 200, Message = $"All members of role '{request.RoleName}' added to discussion.", Data = result });
    }

    /// <summary>
    /// Tier 2 override - Add a specific individual to this case's discussion thread.
    /// Admin/SuperAdmin only.
    /// </summary>
    [HttpPost("participants/individuals")]
    [Authorize]
    public async Task<ActionResult<Response<bool>>> AddIndividualToThread(Guid caseId, [FromBody] AddIndividualRequest request)
    {
        var cmd = new AddIndividualParticipantCommand(caseId, request.UserId);
        var result = await _discussionService.AddIndividualToThreadAsync(cmd, GetUserId(), HttpContext.RequestAborted);
        return Ok(new Response<bool> { StatusCode = 200, Message = "Participant added to discussion.", Data = result });
    }

    /// <summary>
    /// Remove a Tier 2 individually-added participant from the thread.
    /// Only individually-added (non-role-seeded) participants can be removed.
    /// Admin/SuperAdmin only.
    /// </summary>
    [HttpDelete("participants/{userId}")]
    [Authorize]
    public async Task<ActionResult<Response<bool>>> RemoveParticipant(Guid caseId, Guid userId)
    {
        var cmd = new RemoveDiscussionParticipantCommand(caseId, userId);
        var result = await _discussionService.RemoveParticipantAsync(cmd, GetUserId(), HttpContext.RequestAborted);
        return Ok(new Response<bool> { StatusCode = 200, Message = "Participant removed from discussion.", Data = result });
    }
}

// ─── Request Models ───────────────────────────────────────────────────────────

public class PinDiscussionMessageRequest
{
    public bool IsPinned { get; set; }
}

public class AddRoleToThreadRequest
{
    public string RoleName { get; set; } = null!;
}

public class AddIndividualRequest
{
    public Guid UserId { get; set; }
}
