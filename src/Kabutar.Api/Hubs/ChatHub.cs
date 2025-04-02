using Microsoft.AspNetCore.SignalR;

namespace Kabutar.Api.Hubs;

public class ChatHub : Hub
{
    public async Task SendToUser(long userId, string message)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveMessage", message);
    }
}
