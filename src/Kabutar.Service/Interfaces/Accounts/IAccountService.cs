using Kabutar.Service.DTOs.Accounts;


namespace Kabutar.Service.Interfaces.Accounts;

public interface IAccountService
{
    Task<string> LogInAsync(LoginDTO accountLogin);

    Task<bool> RegisterAsync(RegisterDTO accountCreate);

    Task<bool> VerifyEmailAsync(AccountEmailVerify verifyEmail);

    Task SendCodeAsync(SendToEmailDTO sendToEmail);

    Task<bool> VerifyPasswordAsync(AccountResetPasswordDTO userResetPassword);
}
