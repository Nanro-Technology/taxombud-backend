using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Notifications.Commands.DeleteNotification;
using TaxOmbud.Application.Features.Notifications.Commands.MarkAllAsRead;
using TaxOmbud.Application.Features.Notifications.Commands.MarkAsRead;
using TaxOmbud.Application.Features.Notifications.Commands.SendNotification;
using TaxOmbud.Application.Features.Notifications.Queries.GetMyNotifications;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// In-app notification inbox for staff and taxpayer portal users.
/// </summary>
[Authorize]
[Route("api/v1/notifications")]
public class NotificationsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get the authenticated user's notifications (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(unreadOnly, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Mark all notifications for the authenticated user as read.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand(), ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a notification.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Admin: send a notification to a specific user.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendNotificationCommand(request.UserId, request.Title, request.Message), ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(null, result.Value);
    }
}

public record SendNotificationRequest(Guid UserId, string Title, string Message);
