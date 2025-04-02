using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Users;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IIdentityHelperService _identity;

    public UsersController(IUserService userService, IIdentityHelperService identity)
    {
        _userService = userService;
        _identity = identity;
    }

    /// <summary>
    /// Barcha foydalanuvchilar ro‘yxati (admin uchun)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await _userService.GetAllAsync());

    /// <summary>
    /// ID bo‘yicha foydalanuvchini olish
    /// </summary>
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetByIdAsync(long userId)
        => Ok(await _userService.GetIdAsync(userId));

    /// <summary>
    /// Username orqali foydalanuvchini topish
    /// </summary>
    [HttpGet("by-username")]
    public async Task<IActionResult> GetByUsernameAsync([FromQuery] string username)
        => Ok(await _userService.GetUsernameAsync(username));

    /// <summary>
    /// Joriy foydalanuvchi profilingini yangilash
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UserUpdateDTO dto)
    {
        var userId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User not found in token.");
        return Ok(await _userService.UpdateAsync(userId, dto));
    }

    /// <summary>
    /// Profil rasmini yangilash
    /// </summary>
    [HttpPost("image")]
    public async Task<IActionResult> UploadImageAsync([FromForm] AccountImageUploadDTO dto)
    {
        var userId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User not found in token.");
        return Ok(await _userService.ImageUpdateAsync(userId, dto));
    }

    /// <summary>
    /// Joriy foydalanuvchini o‘chirish
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAsync()
    {
        var userId = _identity.GetUserId() ?? throw new UnauthorizedAccessException("User not found in token.");
        return Ok(await _userService.DeleteAsync(userId));
    }
}
