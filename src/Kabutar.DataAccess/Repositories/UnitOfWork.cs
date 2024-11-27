using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Context;

namespace Kabutar.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public IMessageRepository Messages { get; }

        public UnitOfWork(AppDbContext context, IUserRepository users, IMessageRepository messages)
        {
            Users = users;
            Messages = messages;
        }

    }
}
