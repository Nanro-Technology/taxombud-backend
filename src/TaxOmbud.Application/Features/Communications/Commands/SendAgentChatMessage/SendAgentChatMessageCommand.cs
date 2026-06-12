using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Communications;
using Microsoft.AspNetCore.SignalR;
using TaxOmbud.Application.Features.Communications.DTOs;

namespace TaxOmbud.Application.Features.Communications.Commands.SendAgentChatMessage;

public record SendAgentChatMessageCommand : IRequest<Guid>
{
    public Guid AgentChatId { get; set; }
    public string Content { get; set; } = null!;
}

public class SendAgentChatMessageCommandHandler : IRequestHandler<SendAgentChatMessageCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    // We will use MediatR publishing to notify SignalR hub later
    private readonly IMediator _mediator;

    public SendAgentChatMessageCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(SendAgentChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();

        var chat = await _context.AgentChats
            .FirstOrDefaultAsync(c => c.Id == request.AgentChatId && !c.IsDeleted, cancellationToken);

        if (chat == null) throw new ArgumentException("Chat not found");

        var message = new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = request.AgentChatId,
            SenderId = _currentUser.UserId.Value,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        _context.AgentChatMessages.Add(message);
        
        chat.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish event for SignalR
        await _mediator.Publish(new AgentChatMessageSentEvent 
        { 
            ChatId = request.AgentChatId, 
            MessageId = message.Id,
            SenderId = message.SenderId,
            SenderName = _currentUser.FullName ?? "Unknown",
            Content = message.Content,
            CreatedAt = message.CreatedAt
        }, cancellationToken);

        return message.Id;
    }
}

public class AgentChatMessageSentEvent : INotification
{
    public Guid ChatId { get; set; }
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
