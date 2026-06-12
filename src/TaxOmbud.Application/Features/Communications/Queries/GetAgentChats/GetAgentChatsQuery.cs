using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;
using System;
using System.Text.Json;

namespace TaxOmbud.Application.Features.Communications.Queries.GetAgentChats;

public record GetAgentChatsQuery : IRequest<List<AgentChatDto>>;

public class GetAgentChatsQueryHandler : IRequestHandler<GetAgentChatsQuery, List<AgentChatDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetAgentChatsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AgentChatDto>> Handle(GetAgentChatsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null) return new List<AgentChatDto>();
        var userIdString = _currentUser.UserId.Value.ToString();

        // Find chats where the user is a participant
        var chats = await _context.AgentChats
            .Where(c => c.ParticipantIds.Contains(userIdString) && !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<AgentChatDto>();

        foreach (var chat in chats)
        {
            var pIds = JsonSerializer.Deserialize<List<string>>(chat.ParticipantIds) ?? new List<string>();
            var pGuids = pIds.Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty).Where(g => g != Guid.Empty).ToList();

            var participants = await _context.Users
                .Where(u => pGuids.Contains(u.Id))
                .Select(u => new AgentSummaryDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync(cancellationToken);

            result.Add(new AgentChatDto
            {
                Id = chat.Id,
                Topic = chat.Topic,
                IsGroupChat = chat.IsGroupChat,
                Participants = participants,
                CreatedAt = chat.CreatedAt,
                UpdatedAt = chat.UpdatedAt
            });
        }

        return result;
    }
}
