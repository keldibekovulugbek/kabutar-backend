

using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Messages;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
   private readonly DbSet<Message> messages;

    public MessageRepository(DbContext context) : base(context)
    {
        messages = context.Set<Message>();
    }

    // Mark a message as read by its ID
    public async Task MarkMessageAsReadAsync(long messageId)
    {
        var message = await messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    // Get all messages exchanged between two specific users
    public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2)
    {
        return await messages
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.Created) // Assuming 'Created' is a DateTime property on Message
            .ToListAsync();
    }

    // Fetch all unread messages for a specific user
    public async Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId)
    {
        return await messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();
    }

    // Soft delete a message for a user, depending on if the user is the sender or receiver
    public async Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender)
    {
        var message = await messages.FindAsync(messageId);
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

    public async Task<Message> GetLastMessageBetweenUsersAsync(long userId1, long userId2)
    {
        return (await messages
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.Created).FirstOrDefaultAsync())!;
    }

    public async Task<IEnumerable<(User,Message)>> GetAllUsersAndLastMessagesWithOneUserAsync(long userId)
    {
        var sentMessages = messages
            .Where(m => m.SenderId == userId)
            .Select(m => m.Receiver);

        var receivedMessages = messages
            .Where(m => m.ReceiverId == userId)
            .Select(m => m.Sender);

        var allUsers = await sentMessages
            .Union(receivedMessages)
            .Distinct()
            .ToListAsync();

        var lastMessages = new List<(User, Message)>();
        foreach (var user in allUsers)
        {
            var lastMessage = await GetLastMessageBetweenUsersAsync(userId, user.Id);
            lastMessages.Add((user, lastMessage));
        }

        return lastMessages;
    }
}
