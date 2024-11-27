

using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.Domain.Entities.Messages;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Messages;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(DbContext context) : base(context)
    {
    }

    // Mark a message as read by its ID
    public async Task MarkMessageAsReadAsync(long messageId)
    {
        var message = await _context.Set<Message>().FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    // Get all messages exchanged between two specific users
    public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2)
    {
        return await _context.Set<Message>()
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.Created) // Assuming 'Created' is a DateTime property on Message
            .ToListAsync();
    }

    // Fetch all unread messages for a specific user
    public async Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId)
    {
        return await _context.Set<Message>()
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();
    }

    // Soft delete a message for a user, depending on if the user is the sender or receiver
    public async Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender)
    {
        var message = await _context.Set<Message>().FindAsync(messageId);
        if (message != null)
        {
            if (isSender && message.SenderId == userId)
            {
                message.IsDeletedBySender = true;
            }
            else if (!isSender && message.ReceiverId == userId)
            {
                message.IsDeletedByReceiver = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}
