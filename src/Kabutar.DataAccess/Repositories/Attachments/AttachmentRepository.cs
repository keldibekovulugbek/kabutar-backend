using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces.Attachments;
using Kabutar.Domain.Entities.Attachments;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Attachments;

public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(AppDbContext context) : base(context) { }

    public async Task<Attachment?> GetByMessageIdAsync(long messageId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.MessageId == messageId && !a.IsDeleted);
    }

    public async Task<bool> DeleteByMessageIdAsync(long messageId)
    {
        var attachment = await _dbSet
            .FirstOrDefaultAsync(a => a.MessageId == messageId && !a.IsDeleted);

        if (attachment is null) return false;

        attachment.IsDeleted = true;
        attachment.Updated = DateTime.UtcNow;

        _dbSet.Update(attachment);
        await _context.SaveChangesAsync();

        return true;
    }
}
