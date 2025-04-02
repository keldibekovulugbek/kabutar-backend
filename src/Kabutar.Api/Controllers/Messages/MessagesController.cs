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

    [HttpGet("conversation/{userId:long}")]
    public async Task<IActionResult> GetConversationAsync([FromRoute] long userId)
    {
        var myId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in token.");
        var messages = await _messageService.GetConversationAsync(myId, userId);
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
}
