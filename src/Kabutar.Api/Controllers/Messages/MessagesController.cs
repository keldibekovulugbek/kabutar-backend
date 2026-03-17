using Kabutar.Service.DTOs.Messages;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Messages;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IIdentityHelperService _identity;

    public MessageController(IMessageService messageService, IIdentityHelperService identity)
    {
        _messageService = messageService;
        _identity = identity;
    }

    [HttpPost]
    public async Task<IActionResult> SendAsync([FromForm] MessageCreateDTO dto)
    {
        var result = await _messageService.SendMessageAsync(dto);
        return Ok(result);
    }

    [HttpPost("text")]
    public async Task<IActionResult> SendTextAsync([FromBody] MessageCreateDTO dto)
    {
        var result = await _messageService.SendMessageAsync(dto);
        return Ok(result);
    }

    [HttpGet("conversation/{userId:long}")]
    public async Task<IActionResult> GetConversationAsync(
        [FromRoute] long userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;
        var myId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in token.");
        var messages = await _messageService.GetConversationAsync(myId, userId, page, pageSize);
        return Ok(messages);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadAsync()
    {
        var myId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in token.");
        var unread = await _messageService.GetUnreadMessagesAsync(myId);
        return Ok(unread);
    }

    [HttpPut("read/{messageId:long}")]
    public async Task<IActionResult> MarkAsReadAsync([FromRoute] long messageId)
    {
        var result = await _messageService.MarkAsReadAsync(messageId);
        return Ok(result);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetChatUsersAsync()
    {
        var myId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in token.");
        var result = await _messageService.GetAllChatUsersAsync(myId);
        return Ok(result);
    }

    [HttpDelete("chat/{otherUserId:long}")]
    public async Task<IActionResult> ClearChatAsync([FromRoute] long otherUserId, [FromQuery] bool clearForBoth = false)
    {
        var result = await _messageService.ClearChatAsync(otherUserId, clearForBoth);
        return Ok(result);
    }

    [HttpDelete("{messageId:long}")]
    public async Task<IActionResult> DeleteMessageAsync([FromRoute] long messageId, [FromQuery] bool deleteForBoth = false)
    {
        var result = await _messageService.DeleteMessageAsync(messageId, deleteForBoth);
        return Ok(result);
    }
}
