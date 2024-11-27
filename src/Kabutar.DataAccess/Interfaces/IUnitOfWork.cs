
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;

namespace Kabutar.DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    IMessageRepository Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
