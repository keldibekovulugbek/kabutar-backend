using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Interfaces.Attachments;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;

namespace Kabutar.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IUserRepository Users { get; }
    public IMessageRepository Messages { get; }
    public IAttachmentRepository Attachments { get; }

    public UnitOfWork(
        AppDbContext context,
        IUserRepository users,
        IMessageRepository messages,
        IAttachmentRepository attachments)
    {
        _context = context;
        Users = users;
        Messages = messages;
        Attachments = attachments;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
