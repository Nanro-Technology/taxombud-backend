using TaxOmbud.Application.Notifications.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface INotificationsService
{
    Task<Response<object?>> DeleteNotificationAsync(DeleteNotificationCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> MarkAllAsReadAsync(MarkAllAsReadCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> MarkAsReadAsync(MarkAsReadCommand request, CancellationToken cancellationToken = default);
    Task<Response<SentNotificationResponse>> SendNotificationAsync(SendNotificationCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken = default);
    Task<Response<MyNotificationsDto>> GetMyNotificationsAsync(GetMyNotificationsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(GetNotificationPreferencesQuery request, CancellationToken cancellationToken = default);
    Task<Response<int>> GetUnreadNotificationCountAsync(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken = default);
}
