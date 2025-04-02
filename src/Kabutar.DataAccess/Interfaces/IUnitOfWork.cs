using Kabutar.DataAccess.Interfaces.Attachments;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;

namespace Kabutar.DataAccess.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IMessageRepository Messages { get; }
    IAttachmentRepository Attachments { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
