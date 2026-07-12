using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaxOmbud.Application.Chats.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxOmbud.API.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessageDto message);
    Task UserTyping(Guid chatId, Guid userId);
    Task MessageRead(Guid messageId, Guid userId);
    Task UserPresenceChanged(Guid userId, bool isOnline);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ICurrentUser _currentUser;
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();

    public ChatHub(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public static bool IsUserOnline(Guid userId)
    {
        return _connections.TryGetValue(userId, out var conns) && conns.Count > 0;
    }

    public static List<Guid> GetOnlineUsers()
    {
        return _connections.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToList();
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.UserId.HasValue)
        {
            var userId = _currentUser.UserId.Value;
            _connections.AddOrUpdate(userId,
                _ => new HashSet<string> { Context.ConnectionId },
                (_, hs) => { lock (hs) { hs.Add(Context.ConnectionId); } return hs; });

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            await Clients.Others.UserPresenceChanged(userId, true);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUser.UserId.HasValue)
        {
            var userId = _currentUser.UserId.Value;
            if (_connections.TryGetValue(userId, out var hs))
            {
                bool isOffline = false;
                lock (hs)
                {
                    hs.Remove(Context.ConnectionId);
                    if (hs.Count == 0)
                    {
                        isOffline = true;
                    }
                }
                if (isOffline)
                {
                    _connections.TryRemove(userId, out _);
                    await Clients.Others.UserPresenceChanged(userId, false);
                }
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
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
