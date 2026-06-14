using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.UpdateAgentChatPreferences;

public record UpdateAgentChatPreferencesCommand : IRequest<Unit>
{
    public bool DoNotDisturb { get; set; }
    public bool MarkAsAway { get; set; }
    public bool PlayNotificationSound { get; set; }
    public bool ShowBrowserNotifications { get; set; }
}

public class UpdateAgentChatPreferencesCommandHandler : IRequestHandler<UpdateAgentChatPreferencesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateAgentChatPreferencesCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateAgentChatPreferencesCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();

        var prefs = await _context.AgentChatPreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value, cancellationToken);

        if (prefs == null)
        {
            prefs = new AgentChatPreference
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId.Value,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = _currentUser.UserId
            };
            _context.AgentChatPreferences.Add(prefs);
        }

        prefs.DoNotDisturb = request.DoNotDisturb;
        prefs.MarkAsAway = request.MarkAsAway;
        prefs.PlayNotificationSound = request.PlayNotificationSound;
        prefs.ShowBrowserNotifications = request.ShowBrowserNotifications;
        prefs.UpdatedAt = DateTimeOffset.UtcNow;
        prefs.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
