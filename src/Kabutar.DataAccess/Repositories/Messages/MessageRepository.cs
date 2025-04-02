using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.Domain.DTOs.Messages;
using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Messages;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2)
    {
        return await _dbSet
            .Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2 && !m.IsDeletedBySender) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1 && !m.IsDeletedByReceiver)
            )
            .OrderBy(m => m.Created)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId)
    {
        return await _dbSet
            .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeletedByReceiver)
            .ToListAsync();
    }

    public async Task<Message?> GetLastMessageBetweenUsersAsync(long userId1, long userId2)
    {
        return await _dbSet
            .Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2 && !m.IsDeletedBySender) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1 && !m.IsDeletedByReceiver)
            )
            .OrderByDescending(m => m.Created)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserWithLastMessageVM>> GetAllUsersAndLastMessagesWithOneUserAsync(long userId)
    {
        var messages = await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                (m.SenderId == userId && !m.IsDeletedBySender) ||
                (m.ReceiverId == userId && !m.IsDeletedByReceiver)
            )
            .OrderByDescending(m => m.Created)
            .ToListAsync();

        var grouped = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(group =>
            {
                var lastMessage = group.First();
                var contactUser = lastMessage.SenderId == userId ? lastMessage.Receiver : lastMessage.Sender;
                var unreadCount = group.Count(m => m.ReceiverId == userId && !m.IsRead);

                return new UserWithLastMessageVM
                {
                    User = contactUser,
                    LastMessage = lastMessage,
                    UnreadCount = unreadCount
                };
            });

        return grouped;
    }

    public async Task MarkMessageAsReadAsync(long messageId)
    {
        var message = await _dbSet.FindAsync(messageId);
        if (message is not null && !message.IsRead)
        {
            message.IsRead = true;
            message.Updated = DateTime.UtcNow;
            _dbSet.Update(message);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender)
    {
        var message = await _dbSet.FindAsync(messageId);
        if (message is null) return;

        if (isSender) message.IsDeletedBySender = true;
        else message.IsDeletedByReceiver = true;

        message.Updated = DateTime.UtcNow;
        _dbSet.Update(message);
        await _context.SaveChangesAsync();
    }
}
