using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Common;
using Kabutar.Service.DTOs.Users;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Users;
[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IIdentityHelperService _identityHelperService;

    public UsersController(IUserService userService, IIdentityHelperService identityHelperService)
    {
        _userService = userService;
        _identityHelperService = identityHelperService;
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await _userService.GetAllAsync());

    [HttpGet("{userId}"), Authorize]
    public async Task<IActionResult> GetIdAsync(long userId)
        => Ok(await _userService.GetIdAsync(userId));

    [HttpGet("username"), Authorize]
    public async Task<IActionResult> GetUsernameAsync(string username)
    => Ok(await _userService.GetUsernameAsync(username));

    [HttpPut, ]
    public async Task<IActionResult> UpdateAsync([FromForm] UserUpdateDTO userUpdateViewModel)
        => Ok(await _userService.UpdateAsync((long)_identityHelperService.GetUserId()!, userUpdateViewModel));

    [HttpDelete("{userId}"), Authorize]
    public async Task<IActionResult> DeleteAsync(long userId)
        => Ok(await _userService.DeleteAsync(userId));

    [HttpPost("images/upload"), Authorize]
    public async Task<IActionResult> ImageUpdateAsync(int id, [FromForm] AccountImageUploadDTO dto)
        => Ok(await _userService.ImageUpdateAsync((long)_identityHelperService.GetUserId()!, dto));

}