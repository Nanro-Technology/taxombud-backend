using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;

namespace TaxOmbud.Application.Features.Communications.Queries.GetChatMessages;

public record GetChatMessagesQuery(Guid ChatId) : IRequest<List<AgentChatMessageDto>>;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<AgentChatMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChatMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgentChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.AgentChatMessages
            .Where(m => m.AgentChatId == request.ChatId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // Fetch user names for senders
        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, cancellationToken);

        return messages.Select(m => new AgentChatMessageDto
        {
            Id = m.Id,
            AgentChatId = m.AgentChatId,
            SenderId = m.SenderId,
            SenderName = users.GetValueOrDefault(m.SenderId) ?? "Unknown",
            Content = m.Content,
            IsPinned = m.IsPinned,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}
