using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Chats.Commands;

public record CreateChatCommand(string? Topic, bool IsGroupChat, List<Guid> ParticipantIds) : IRequest<Guid>;

public class CreateChatCommandHandler : IRequestHandler<CreateChatCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateChatCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var participants = new List<string>();
        if (_currentUser.UserId.HasValue && _currentUser.UserId.Value != Guid.Empty)
        {
            participants.Add(_currentUser.UserId.Value.ToString());
        }
        foreach(var id in request.ParticipantIds)
        {
            var s = id.ToString();
            if (!participants.Contains(s)) participants.Add(s);
        }

        var chat = new AgentChat
        {
            Topic = request.Topic,
            IsGroupChat = request.IsGroupChat,
            ParticipantIds = JsonSerializer.Serialize(participants)
        };

        _context.AgentChats.Add(chat);
        await _context.SaveChangesAsync(cancellationToken);

        return chat.Id;
    }
}
