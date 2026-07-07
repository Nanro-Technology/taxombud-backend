using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Notifications.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Services;

public class NotificationsService : INotificationsService
{
    private readonly IGenericRepository<Notification> _notificationRepo;
    private readonly IGenericRepository<NotificationPreference> _preferenceRepo;
    private readonly ICurrentUser _currentUser;

    public NotificationsService(
        IGenericRepository<Notification> notificationRepo,
        IGenericRepository<NotificationPreference> preferenceRepo,
        ICurrentUser currentUser)
    {
        _notificationRepo = notificationRepo;
        _preferenceRepo = preferenceRepo;
        _currentUser = currentUser;
    }

    public async Task<Response<object?>> DeleteNotificationAsync(DeleteNotificationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var notification = await _notificationRepo.FindAsync(x => x.Id == request.Id);
            if (notification == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Notification not found.";
                return response;
            }

            await _notificationRepo.RemoveAsync(notification);
            await _notificationRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Notification deleted successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> MarkAsReadAsync(MarkAsReadCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var notification = await _notificationRepo.FindAsync(x => x.Id == request.Id);
            if (notification == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Notification not found.";
                return response;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            await _notificationRepo.UpdateAsync(notification);
            await _notificationRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Notification marked as read.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> MarkAllAsReadAsync(MarkAllAsReadCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = "User is not authenticated.";
            return response;
        }
        try
        {
            var unread = await _notificationRepo.FindAllAsync(
                n => n.UserId == currentUserId.Value && !n.IsRead);

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTimeOffset.UtcNow;
                await _notificationRepo.UpdateAsync(n);
            }

            await _notificationRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "All notifications marked as read.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<SentNotificationResponse>> SendNotificationAsync(SendNotificationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SentNotificationResponse>();
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new SentNotificationResponse(notification.Id, notification.Title, notification.CreatedAt);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = "User is not authenticated.";
            return response;
        }
        try
        {
            var existingPrefs = await _preferenceRepo.FindAllAsync(p => p.UserId == currentUserId.Value);

            foreach (var prefUpdate in request.Preferences)
            {
                var existing = existingPrefs.FirstOrDefault(p => p.EventType == prefUpdate.Type);
                if (existing != null)
                {
                    existing.EmailEnabled = prefUpdate.Email;
                    existing.SmsEnabled = prefUpdate.Sms;
                    existing.InAppEnabled = prefUpdate.InApp;
                    await _preferenceRepo.UpdateAsync(existing);
                }
                else
                {
                    await _preferenceRepo.AddAsync(new NotificationPreference
                    {
                        UserId = currentUserId.Value,
                        EventType = prefUpdate.Type,
                        EmailEnabled = prefUpdate.Email,
                        SmsEnabled = prefUpdate.Sms,
                        InAppEnabled = prefUpdate.InApp
                    });
                }
            }

            await _preferenceRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<MyNotificationsDto>> GetMyNotificationsAsync(GetMyNotificationsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<MyNotificationsDto>();
        try
        {
            var userId = _currentUser.UserId ?? Guid.Empty;

            var query = _notificationRepo.Query()
                .Where(n => n.UserId == userId)
                .AsNoTracking();

            if (request.UnreadOnly == true)
                query = query.Where(n => !n.IsRead);

            var total = await query.CountAsync(cancellationToken);
            var unreadCount = await _notificationRepo.CountAsync(n => n.UserId == userId && !n.IsRead);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationItemDto(
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.ReadAt,
                    n.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new MyNotificationsDto(items, total, unreadCount, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(GetNotificationPreferencesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<NotificationPreferenceDto>>();
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = "User is not authenticated.";
            return response;
        }
        try
        {
            var preferences = await _preferenceRepo.Query()
                .Where(p => p.UserId == currentUserId.Value)
                .AsNoTracking()
                .Select(p => new NotificationPreferenceDto(p.EventType, true, p.EmailEnabled, p.SmsEnabled, p.InAppEnabled))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = preferences;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<int>> GetUnreadNotificationCountAsync(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<int>();
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = "User is not authenticated.";
            return response;
        }
        try
        {
            var count = await _notificationRepo.CountAsync(
                n => n.UserId == currentUserId.Value && !n.IsRead);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = count;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}
