using Kabutar.Domain.DTOs.Messages;
using Kabutar.Domain.Entities.Messages;
namespace Kabutar.DataAccess.Interfaces.Messages;

public interface IMessageRepository : IGenericRepository<Message>
{
    /// <summary>
    /// Foydalanuvchilar o‘rtasidagi barcha xabarlar
    /// </summary>
    Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2);

    /// <summary>
    /// Foydalanuvchi uchun hali o‘qilmagan xabarlar
    /// </summary>
    Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId);

    /// <summary>
    /// Ikki foydalanuvchi o‘rtasidagi oxirgi xabar (null bo‘lishi mumkin)
    /// </summary>
    Task<Message?> GetLastMessageBetweenUsersAsync(long userId1, long userId2);

    /// <summary>
    /// Foydalanuvchiga yozgan barcha userlar va ular bilan oxirgi xabar
    /// </summary>
    Task<IEnumerable<UserWithLastMessageVM>> GetAllUsersAndLastMessagesWithOneUserAsync(long userId);

    /// <summary>
    /// Xabarni o‘qilgan deb belgilash
    /// </summary>
    Task MarkMessageAsReadAsync(long messageId);

    /// <summary>
    /// Foydalanuvchi uchun xabarni o‘chirish (soft delete)
    /// </summary>
    Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender);
}
