using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.API.Hubs;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.Commands.SendAgentChatMessage;

namespace TaxOmbud.API.EventHandlers;

public class AgentChatMessageSentEventHandler : INotificationHandler<AgentChatMessageSentEvent>
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IApplicationDbContext _context;

    public AgentChatMessageSentEventHandler(IHubContext<ChatHub> hubContext, IApplicationDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task Handle(AgentChatMessageSentEvent notification, CancellationToken cancellationToken)
    {
        // Get chat participants
        var chat = await _context.AgentChats
            .FirstOrDefaultAsync(c => c.Id == notification.ChatId, cancellationToken);

        if (chat == null) return;

        var participantIds = JsonSerializer.Deserialize<string[]>(chat.ParticipantIds);
        if (participantIds == null) return;

        // Broadcast to all participants using their UserId group
        await _hubContext.Clients.Groups(participantIds).SendAsync("ReceiveMessage", notification, cancellationToken);
    }
}
