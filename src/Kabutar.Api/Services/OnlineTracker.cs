using Kabutar.Api.Hubs;
using Kabutar.Service.Interfaces.Common;

namespace Kabutar.Api.Services;

public class OnlineTracker : IOnlineTracker
{
    public bool IsOnline(long userId) => ChatHub.IsUserOnline(userId);
}
