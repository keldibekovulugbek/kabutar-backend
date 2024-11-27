﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Kabutar.Domain.Entities.Messages;

namespace Kabutar.DataAccess.Interfaces.Messages
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(long userId1, long userId2);
        Task<IEnumerable<Message>> GetUnreadMessagesForUserAsync(long userId);
        Task MarkMessageAsReadAsync(long messageId);
        Task DeleteMessageForUserAsync(long messageId, long userId, bool isSender);
    }
}
