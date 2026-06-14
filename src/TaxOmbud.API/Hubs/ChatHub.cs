using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Chats.DTOs;

namespace TaxOmbud.API.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessageDto message);
    Task UserTyping(Guid chatId, Guid userId);
    Task MessageRead(Guid messageId, Guid userId);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ICurrentUser _currentUser;

    public ChatHub(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.UserId.HasValue)
        {
            // Add user to a group based on their UserId so we can target them specifically
            await Groups.AddToGroupAsync(Context.ConnectionId, _currentUser.UserId.Value.ToString());
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUser.UserId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _currentUser.UserId.Value.ToString());
        }
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendTypingIndicator(Guid chatId, string[] participantIds)
    {
        if (!_currentUser.UserId.HasValue) return;
        
        foreach (var pId in participantIds)
        {
            if (pId != _currentUser.UserId.Value.ToString())
            {
                await Clients.Group(pId).UserTyping(chatId, _currentUser.UserId.Value);
            }
        }
    }
}
