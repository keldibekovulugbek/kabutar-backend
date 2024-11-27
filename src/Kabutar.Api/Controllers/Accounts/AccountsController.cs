using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.Interfaces.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kabutar.Api.Controllers.Accounts;

[Route("api/accounts")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> RegistrateAsync([FromForm] RegisterDTO accountCreateViewModel)
        => Ok(await _accountService.RegisterAsync(accountCreateViewModel));

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromForm] LoginDTO accountLoginViewModel)
        => Ok(new { Token = await _accountService.LogInAsync(accountLoginViewModel) });

    [HttpPost("verifyemail")]
    public async Task<IActionResult> VerifyEmail([FromForm] AccountEmailVerify verifyEmail)
        => Ok(await _accountService.VerifyEmailAsync(verifyEmail));

    [HttpPost("reset-password"), AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordAsync([FromQuery] AccountResetPasswordDTO userReset)
    => Ok(await _accountService.VerifyPasswordAsync(userReset));

    [HttpPost("sendcode"), AllowAnonymous]
    public async Task<IActionResult> SendToEmail([FromBody] SendToEmailDTO sendTo)
    {
        await _accountService.SendCodeAsync(sendTo);
        return Ok();
    }
}
