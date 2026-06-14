using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.CreateAgentChat;

public record CreateAgentChatCommand : IRequest<Guid>
{
    public string? Topic { get; set; }
    public bool IsGroupChat { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class CreateAgentChatCommandHandler : IRequestHandler<CreateAgentChatCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateAgentChatCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAgentChatCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();

        var participants = request.ParticipantIds.ToList();
        if (!participants.Contains(_currentUser.UserId.Value))
        {
            participants.Add(_currentUser.UserId.Value);
        }

        // Check if a direct message chat already exists between these 2 users
        if (!request.IsGroupChat && participants.Count == 2)
        {
            var p1 = participants[0].ToString();
            var p2 = participants[1].ToString();

            var existingChat = await _context.AgentChats
                .Where(c => !c.IsGroupChat && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            var chat = existingChat.FirstOrDefault(c => 
                c.ParticipantIds.Contains(p1) && 
                c.ParticipantIds.Contains(p2) &&
                JsonSerializer.Deserialize<List<string>>(c.ParticipantIds)?.Count == 2);

            if (chat != null)
            {
                return chat.Id;
            }
        }

        var newChat = new AgentChat
        {
            Id = Guid.NewGuid(),
            Topic = request.Topic,
            IsGroupChat = request.IsGroupChat,
            ParticipantIds = JsonSerializer.Serialize(participants.Select(p => p.ToString())),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        _context.AgentChats.Add(newChat);
        await _context.SaveChangesAsync(cancellationToken);

        return newChat.Id;
    }
}
