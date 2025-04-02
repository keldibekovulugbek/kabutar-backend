using Kabutar.DataAccess.Interfaces;
using Kabutar.Service.DTOs.Attachments;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Interfaces.Attachments;
using System.Net;

namespace Kabutar.Service.Services.Attachments;

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttachmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AttachmentViewModel?> GetByMessageIdAsync(long messageId)
    {
        var attachment = await _unitOfWork.Attachments.GetByMessageIdAsync(messageId);

        if (attachment is null)
            throw new StatusCodeException(HttpStatusCode.NotFound, "Attachment not found");

        return new AttachmentViewModel
        {
            FilePath = attachment.FilePath,
            FileType = attachment.FileType,
            MimeType = attachment.MimeType
        };
    }

    public async Task<bool> DeleteByMessageIdAsync(long messageId)
    {
        return await _unitOfWork.Attachments.DeleteByMessageIdAsync(messageId);
    }
}
