using Kabutar.Service.DTOs.Accounts;

namespace Kabutar.Service.Interfaces.Accounts;

public interface IAccountService
{
    Task<string> LogInAsync(LoginDTO dto);
    Task<bool> RegisterAsync(RegisterDTO dto);
    Task<bool> VerifyEmailAsync(AccountEmailVerify dto);
    Task SendCodeAsync(SendToEmailDTO dto);
    Task<bool> ResetPasswordAsync(AccountResetPasswordDTO dto);
}
