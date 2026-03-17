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

    [HttpGet("message/{messageId:long}")]
    public async Task<IActionResult> GetByMessageIdAsync([FromRoute] long messageId)
    {
        var result = await _attachmentService.GetByMessageIdAsync(messageId);
        return Ok(result);
    }

    [HttpGet("download/{messageId:long}")]
    public async Task<IActionResult> DownloadAsync([FromRoute] long messageId)
    {
        var attachment = await _attachmentService.GetByMessageIdAsync(messageId);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        var wwwrootBase = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
        var filePath = Path.GetFullPath(Path.Combine(wwwrootBase, attachment.FilePath.TrimStart('/', '\\')));

        if (!filePath.StartsWith(wwwrootBase, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid file path." });

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "File not found on server" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);

        return File(fileBytes, attachment.MimeType, fileName);
    }

    [HttpDelete("message/{messageId:long}")]
    public async Task<IActionResult> DeleteByMessageIdAsync([FromRoute] long messageId)
    {
        var result = await _attachmentService.DeleteByMessageIdAsync(messageId);
        return Ok(result);
    }
}
