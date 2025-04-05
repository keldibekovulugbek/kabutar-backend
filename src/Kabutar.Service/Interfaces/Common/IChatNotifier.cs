namespace Kabutar.Service.Interfaces.Common;

public interface IChatNotifier
{
    Task SendMessageToUserAsync(long userId, object payload);
}
