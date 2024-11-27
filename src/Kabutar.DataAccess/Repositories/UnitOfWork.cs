using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces;

namespace Kabutar.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        public IUserRepository Users { get; }
        public IMessageRepository Messages { get; }

        public UnitOfWork(DbContext context, IUserRepository users, IMessageRepository messages)
        {
            _context = context;
            Users = users;
            Messages = messages;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
