using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Notifications.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Services;

public class NotificationsService : INotificationsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public NotificationsService(
        IApplicationDbContext context,
        ICurrentUser currentUser
    )
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Response<object?>> DeleteNotificationAsync(DeleteNotificationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (notification == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Notification not found.";
                return response;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);

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
            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (notification == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Notification not found.";
                return response;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

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
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == currentUserId.Value && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                n.ReadAt = DateTimeOffset.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

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
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

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
            var existingPrefs = await _context.NotificationPreferences
                .Where(p => p.UserId == currentUserId.Value)
                .ToListAsync(cancellationToken);

            foreach (var prefUpdate in request.Preferences)
            {
                var existing = existingPrefs.FirstOrDefault(p => p.EventType == prefUpdate.Type);
                if (existing != null)
                {
                    existing.EmailEnabled = prefUpdate.Email;
                    existing.SmsEnabled = prefUpdate.Sms;
                    existing.InAppEnabled = prefUpdate.InApp;
                }
                else
                {
                    _context.NotificationPreferences.Add(new NotificationPreference
                    {
                        UserId = currentUserId.Value,
                        EventType = prefUpdate.Type,
                        EmailEnabled = prefUpdate.Email,
                        SmsEnabled = prefUpdate.Sms,
                        InAppEnabled = prefUpdate.InApp
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

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

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsNoTracking()
                .AsQueryable();

            if (request.UnreadOnly == true)
                query = query.Where(n => !n.IsRead);

            var total = await query.CountAsync(cancellationToken);
            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

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

            var dto = new MyNotificationsDto(items, total, unreadCount, request.Page, request.PageSize);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = dto;
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
            var preferences = await _context.NotificationPreferences
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
            var count = await _context.Notifications
                .Where(n => n.UserId == currentUserId.Value && !n.IsRead)
                .CountAsync(cancellationToken);

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