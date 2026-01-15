using Kabutar.Service.Interfaces.Users;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Kabutar.Api.Hubs;

public class ChatHub : Hub
{
    // Connected users (connectionId ↔ userId)
    private static readonly ConcurrentDictionary<string, long> _connections = new();
    private readonly IServiceProvider _serviceProvider;

    public ChatHub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // ✅ On connected
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var userIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdStr, out var userId))
            {
                _connections.TryAdd(Context.ConnectionId, userId);

                // Update LastActive
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.UpdateLastActiveAsync(userId);
                }

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
            // Update LastActive when disconnecting
            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.UpdateLastActiveAsync(userId);
            }

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
        var userIdStr = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdStr, out var id) ? id : throw new UnauthorizedAccessException("User ID not found in token");
    }

    // ✅ (Optional) Check online status
    public static bool IsUserOnline(long userId)
    {
        return _connections.Values.Contains(userId);
    }
}
