using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Notifications.DTOs;

namespace TaxOmbud.Api.Controllers;

public record SendNotificationRequest(Guid UserId, string Title, string Message);
public record UpdateNotificationPreferencesRequest(List<PreferenceUpdateDto> Preferences);

/// <summary>
/// In-app notification inbox for staff and taxpayer portal users.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService _notificationsService;

    public NotificationsController(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
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
        var result = await _notificationsService.GetMyNotificationsAsync(new GetMyNotificationsQuery(unreadOnly, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get the number of unread notifications for the user.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await _notificationsService.GetUnreadNotificationCountAsync(new GetUnreadNotificationCountQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get the authenticated user's notification preferences.</summary>
    [HttpGet("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var result = await _notificationsService.GetNotificationPreferencesAsync(new GetNotificationPreferencesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Admin: send a notification to a specific user.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request, CancellationToken ct)
    {
        var result = await _notificationsService.SendNotificationAsync(new SendNotificationCommand(request.UserId, request.Title, request.Message), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var result = await _notificationsService.MarkAsReadAsync(new MarkAsReadCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Mark all notifications for the authenticated user as read.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var result = await _notificationsService.MarkAllAsReadAsync(new MarkAllAsReadCommand(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update the authenticated user's notification preferences.</summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesRequest request, CancellationToken ct)
    {
        var result = await _notificationsService.UpdateNotificationPreferencesAsync(new UpdateNotificationPreferencesCommand(request.Preferences), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a notification.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken ct)
    {
        var result = await _notificationsService.DeleteNotificationAsync(new DeleteNotificationCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
