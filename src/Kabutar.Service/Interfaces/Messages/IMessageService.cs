using Kabutar.Service.DTOs.Messages;

namespace Kabutar.Service.Interfaces.Messages;

public interface IMessageService
{
    Task<bool> SendMessageAsync(MessageCreateDTO dto);
    Task<IEnumerable<MessageViewModel>> GetConversationAsync(long userId1, long userId2, int page = 1, int pageSize = 50);
    Task<IEnumerable<MessageViewModel>> GetUnreadMessagesAsync(long userId);
    Task<bool> MarkAsReadAsync(long messageId);
    Task<bool> DeleteMessageAsync(long messageId, bool deleteForBoth = false);
    Task<bool> ClearChatAsync(long otherUserId, bool clearForBoth = false);
    Task<IEnumerable<UserWithLastMessageDTO>> GetAllChatUsersAsync(long userId);
}
