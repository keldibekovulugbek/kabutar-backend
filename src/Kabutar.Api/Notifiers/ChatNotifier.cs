using Kabutar.Service.Interfaces.Common;
using Kabutar.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Kabutar.Api.Notifiers;

public class ChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub> _hub;

    public ChatNotifier(IHubContext<ChatHub> hub)
    {
        _hub = hub;
    }

    public async Task SendMessageToUserAsync(long userId, object payload)
    {
        await _hub.Clients.User(userId.ToString())
                  .SendAsync("ReceiveMessage", payload);
    }
}
