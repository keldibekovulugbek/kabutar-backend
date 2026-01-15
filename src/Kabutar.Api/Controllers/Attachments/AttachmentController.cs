using Kabutar.Service.Interfaces.Attachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Attachments;

[Route("api/attachments")]
[ApiController]
[Authorize]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Get an attachment by message ID
    /// </summary>
    [HttpGet("message/{messageId:long}")]
    public async Task<IActionResult> GetByMessageIdAsync([FromRoute] long messageId)
    {
        var result = await _attachmentService.GetByMessageIdAsync(messageId);
        return Ok(result);
    }

    /// <summary>
    /// Download attachment file
    /// </summary>
    [HttpGet("download/{messageId:long}")]
    public async Task<IActionResult> DownloadAsync([FromRoute] long messageId)
    {
        var attachment = await _attachmentService.GetByMessageIdAsync(messageId);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "File not found on server" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);

        return File(fileBytes, attachment.MimeType, fileName);
    }

    /// <summary>
    /// Delete an attachment by message ID
    /// </summary>
    [HttpDelete("message/{messageId:long}")]
    public async Task<IActionResult> DeleteByMessageIdAsync([FromRoute] long messageId)
    {
        var result = await _attachmentService.DeleteByMessageIdAsync(messageId);
        return Ok(result);
    }
}
