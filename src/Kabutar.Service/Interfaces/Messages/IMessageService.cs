using Kabutar.Service.DTOs.Messages;

namespace Kabutar.Service.Interfaces.Messages;

public interface IMessageService
{
    Task<bool> SendMessageAsync(MessageCreateDTO dto);
    Task<IEnumerable<MessageViewModel>> GetConversationAsync(long userId1, long userId2);
    Task<IEnumerable<MessageViewModel>> GetUnreadMessagesAsync(long userId);
    Task<bool> MarkAsReadAsync(long messageId);
    Task<IEnumerable<UserWithLastMessageDTO>> GetAllChatUsersAsync(long userId);
}
