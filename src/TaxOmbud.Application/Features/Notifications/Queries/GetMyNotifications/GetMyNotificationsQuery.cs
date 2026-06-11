using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Notifications.Queries.GetMyNotifications;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetMyNotificationsQuery(
    bool? UnreadOnly,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<MyNotificationsDto>>;

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, Result<MyNotificationsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyNotificationsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<MyNotificationsDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
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
        return Result<MyNotificationsDto>.Success(dto);
    }
}
