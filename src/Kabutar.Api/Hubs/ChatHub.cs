using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Kabutar.Api.Hubs;

public class ChatHub : Hub
{
    // Connected users (connectionId ↔ userId)
    private static readonly ConcurrentDictionary<string, long> _connections = new();

    // ✅ On connected
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var userIdStr = user.FindFirst("Id")?.Value;

            if (long.TryParse(userIdStr, out var userId))
            {
                _connections.TryAdd(Context.ConnectionId, userId);

                // Real-time event
                await Clients.All.SendAsync("UserConnected", userId);
            }
        }

        await base.OnConnectedAsync();
    }

    // ✅ On disconnected
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var userId))
        {
            // Real-time event
            await Clients.All.SendAsync("UserDisconnected", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ✅ Send message to specific user
    public async Task SendToUser(long receiverId, string message)
    {
        var senderId = GetUserId();

        var payload = new
        {
            SenderId = senderId,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", payload);
    }

    // ✅ Optional: Send message to all users
    public async Task SendToAll(string message)
    {
        var senderId = GetUserId();

        await Clients.All.SendAsync("BroadcastMessage", new
        {
            SenderId = senderId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    // ✅ Helper
    private long GetUserId()
    {
        var userIdStr = Context.User?.FindFirst("Id")?.Value;
        return long.TryParse(userIdStr, out var id) ? id : throw new UnauthorizedAccessException("User ID not found in token");
    }

    // ✅ (Optional) Check online status
    public static bool IsUserOnline(long userId)
    {
        return _connections.Values.Contains(userId);
    }
}
