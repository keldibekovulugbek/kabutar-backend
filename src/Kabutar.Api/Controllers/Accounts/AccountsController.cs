using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.Interfaces.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Accounts;

[ApiController]
[Route("api/account")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Foydalanuvchini ro'yxatdan o'tkazish
    /// </summary>
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO dto)
    {
        var result = await _accountService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Login (JWT token qaytaradi)
    /// </summary>
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDTO dto)
    {
        var token = await _accountService.LogInAsync(dto);
        return Ok(new { Token = token });
    }

    /// <summary>
    /// Email manzilga tasdiqlash kodi yuborish
    /// </summary>
    [HttpPost("send-code"), AllowAnonymous]
    public async Task<IActionResult> SendCodeAsync([FromBody] SendToEmailDTO dto)
    {
        await _accountService.SendCodeAsync(dto);
        return Ok(new { Message = "Code sent to email." });
    }

    /// <summary>
    /// Email tasdiqlash (kodi bilan)
    /// </summary>
    [HttpPost("verify-email"), AllowAnonymous]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] AccountEmailVerify dto)
    {
        var result = await _accountService.VerifyEmailAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Parolni yangilash (tasdiqlovchi koddan so'ng)
    /// </summary>
    [HttpPost("reset-password"), AllowAnonymous]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] AccountResetPasswordDTO dto)
    {
        var result = await _accountService.ResetPasswordAsync(dto);
        return Ok(result);
    }
}
