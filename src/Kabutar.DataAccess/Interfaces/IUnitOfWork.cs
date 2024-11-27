
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;

namespace Kabutar.DataAccess.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }

    IMessageRepository Messages { get; }
}
