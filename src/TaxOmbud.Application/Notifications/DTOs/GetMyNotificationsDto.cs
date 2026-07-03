using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Notifications.DTOs;

public record GetMyNotificationsQuery(
    bool? UnreadOnly,
    int Page = 1,
    int PageSize = 20
) ;

public record MyNotificationsDto(
    IEnumerable<NotificationItemDto> Items,
    int Total,
    int UnreadCount,
    int Page,
    int PageSize
);

public record NotificationItemDto(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt
);
