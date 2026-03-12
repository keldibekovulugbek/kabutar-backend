using Kabutar.Domain.DTOs.Messages;
using Kabutar.Domain.Entities.Messages;
namespace Kabutar.DataAccess.Interfaces.Messages;

public interface IMessageRepository : IGenericRepository<Message>
{


    Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2);


    Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId);


    Task<Message?> GetLastMessageBetweenUsersAsync(long userId1, long userId2);


    Task<IEnumerable<UserWithLastMessageVM>> GetAllUsersAndLastMessagesWithOneUserAsync(long userId);


    Task MarkMessageAsReadAsync(long messageId);


    Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender);


    Task DeleteMessageForBothAsync(long messageId);


    Task<List<Message>> SearchMessagesAsync(string searchText, long currentUserId, int limit = 20);


    Task<List<Message>> GetAllMessagesForUserAsync(long userId, int limit = 500);


    Task ClearChatForUserAsync(long userId, long otherUserId);


    Task ClearChatForBothAsync(long userId1, long userId2);
}
