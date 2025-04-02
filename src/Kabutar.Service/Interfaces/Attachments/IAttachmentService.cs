using Kabutar.Service.DTOs.Attachments;

namespace Kabutar.Service.Interfaces.Attachments;

public interface IAttachmentService
{
    Task<AttachmentViewModel?> GetByMessageIdAsync(long messageId);
    Task<bool> DeleteByMessageIdAsync(long messageId);
}
