using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Notifications.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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
