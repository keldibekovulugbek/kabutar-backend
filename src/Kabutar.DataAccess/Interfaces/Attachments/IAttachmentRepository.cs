using Kabutar.Domain.Entities.Attachments;

namespace Kabutar.DataAccess.Interfaces.Attachments;

public interface IAttachmentRepository : IGenericRepository<Attachment>
{
    /// <summary>
    /// Biror xabarga biriktirilgan faylni olish
    /// </summary>
    Task<Attachment?> GetByMessageIdAsync(long messageId);

    /// <summary>
    /// Faylni soft delete qilish
    /// </summary>
    Task<bool> DeleteByMessageIdAsync(long messageId);
}
