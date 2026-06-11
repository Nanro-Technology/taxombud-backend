using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Features.Notifications.Queries.GetNotificationPreferences;

public record GetNotificationPreferencesQuery() : IRequest<Result<List<NotificationPreferenceDto>>>;

public record NotificationPreferenceDto(string EventType, bool EmailEnabled, bool SmsEnabled, bool InAppEnabled);

public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, Result<List<NotificationPreferenceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNotificationPreferencesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationPreferenceDto>>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
            return Result<List<NotificationPreferenceDto>>.Failure("User is not authenticated.");

        var preferences = await _context.NotificationPreferences
            .Where(p => p.UserId == currentUserId.Value)
            .AsNoTracking()
            .Select(p => new NotificationPreferenceDto(p.EventType, p.EmailEnabled, p.SmsEnabled, p.InAppEnabled))
            .ToListAsync(cancellationToken);

        return Result<List<NotificationPreferenceDto>>.Success(preferences);
    }
}
