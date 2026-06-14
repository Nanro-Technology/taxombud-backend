using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;

namespace TaxOmbud.Application.Features.Communications.Queries.GetAgentChatPreferences;

public record GetAgentChatPreferencesQuery : IRequest<AgentChatPreferenceDto>;

public class GetAgentChatPreferencesQueryHandler : IRequestHandler<GetAgentChatPreferencesQuery, AgentChatPreferenceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetAgentChatPreferencesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AgentChatPreferenceDto> Handle(GetAgentChatPreferencesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null) return new AgentChatPreferenceDto();

        var prefs = await _context.AgentChatPreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value, cancellationToken);

        if (prefs == null)
        {
            // Return defaults
            return new AgentChatPreferenceDto
            {
                UserId = _currentUser.UserId.Value,
                DoNotDisturb = false,
                MarkAsAway = false,
                PlayNotificationSound = true,
                ShowBrowserNotifications = true
            };
        }

        return new AgentChatPreferenceDto
        {
            UserId = prefs.UserId,
            DoNotDisturb = prefs.DoNotDisturb,
            MarkAsAway = prefs.MarkAsAway,
            PlayNotificationSound = prefs.PlayNotificationSound,
            ShowBrowserNotifications = prefs.ShowBrowserNotifications
        };
    }
}
