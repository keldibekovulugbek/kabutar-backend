using Kabutar.Service.Interfaces.Users;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Kabutar.Api.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, long> _connections = new();
    private readonly IServiceProvider _serviceProvider;

    public ChatHub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var userIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdStr, out var userId))
            {
                _connections.TryAdd(Context.ConnectionId, userId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.UpdateLastActiveAsync(userId);
                }

                await Clients.All.SendAsync("UserConnected", userId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var userId))
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.UpdateLastActiveAsync(userId);
            }

            await Clients.All.SendAsync("UserDisconnected", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task StartTyping(long receiverId)
    {
        var senderId = GetUserId();
        await Clients.User(receiverId.ToString()).SendAsync("TypingStarted", senderId);
    }

    public async Task StopTyping(long receiverId)
    {
        var senderId = GetUserId();
        await Clients.User(receiverId.ToString()).SendAsync("TypingStopped", senderId);
    }

    public async Task MarkMessageRead(long messageId, long senderId)
    {
        await Clients.User(senderId.ToString()).SendAsync("MessageRead", messageId);
    }

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

    private long GetUserId()
    {
        var userIdStr = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdStr, out var id) ? id : throw new UnauthorizedAccessException("User ID not found in token");
    }

    public static bool IsUserOnline(long userId)
    {
        return _connections.Values.Contains(userId);
    }

    public static long? GetUserIdByConnectionId(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var id) ? id : null;
    }
}
