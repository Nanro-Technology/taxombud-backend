using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Features.Notifications.Commands.UpdateNotificationPreferences;

public record UpdateNotificationPreferencesCommand(List<PreferenceUpdateDto> Preferences) : IRequest<Result<Unit>>;

public record PreferenceUpdateDto(string EventType, bool EmailEnabled, bool SmsEnabled, bool InAppEnabled);

public class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateNotificationPreferencesCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null)
            return Result<Unit>.Failure("User is not authenticated.");

        var existingPrefs = await _context.NotificationPreferences
            .Where(p => p.UserId == currentUserId.Value)
            .ToListAsync(cancellationToken);

        foreach (var prefUpdate in request.Preferences)
        {
            var existing = existingPrefs.FirstOrDefault(p => p.EventType == prefUpdate.EventType);
            if (existing != null)
            {
                existing.EmailEnabled = prefUpdate.EmailEnabled;
                existing.SmsEnabled = prefUpdate.SmsEnabled;
                existing.InAppEnabled = prefUpdate.InAppEnabled;
            }
            else
            {
                _context.NotificationPreferences.Add(new NotificationPreference
                {
                    UserId = currentUserId.Value,
                    EventType = prefUpdate.EventType,
                    EmailEnabled = prefUpdate.EmailEnabled,
                    SmsEnabled = prefUpdate.SmsEnabled,
                    InAppEnabled = prefUpdate.InAppEnabled
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
