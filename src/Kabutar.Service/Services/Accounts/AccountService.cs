
using Kabutar.DataAccess.Interfaces;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Common;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Accounts;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Security;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Kabutar.Service.Services.Accounts;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthManager _authManager;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;

    public AccountService(IUnitOfWork unitOfWork, IAuthManager authManager,
        IMemoryCache cache, IEmailService emailService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _authManager = authManager;
        _cache = cache;
        _emailService = emailService;
        _fileService = fileService;
    }

    public async Task<string> LogInAsync(LoginDTO accountLogin)
    {
        if (accountLogin.UsernameOrEmail.Contains('@'))
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(accountLogin.UsernameOrEmail.ToLower().Trim());

            if (user is null) throw new StatusCodeException(HttpStatusCode.NotFound, message: "email is wrong");

            if (user.IsEmailVerified is false)
                throw new StatusCodeException(HttpStatusCode.BadRequest, message: "email did not verified");

            if (PasswordHasher.Verify(accountLogin.Password, user.PasswordHash))
                return _authManager.GenerateToken(user);
            else throw new StatusCodeException(HttpStatusCode.BadRequest, message: "password is wrong");
        }
        else
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(accountLogin.UsernameOrEmail);

            if (user is null) throw new StatusCodeException(HttpStatusCode.NotFound, message: "username is wrong");

            if (user.IsEmailVerified is false)
                throw new StatusCodeException(HttpStatusCode.BadRequest, message: "email did not verified");

            if (PasswordHasher.Verify(accountLogin.Password, user.PasswordHash))
                return _authManager.GenerateToken(user);
            else throw new StatusCodeException(HttpStatusCode.BadRequest, message: "password is wrong");
        }

    }

    public async Task<bool> RegisterAsync(RegisterDTO accountCreate)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(accountCreate.Email.ToLower());
        if (user is not null) throw new StatusCodeException(HttpStatusCode.BadRequest, message: "user already exist");
        
        var newUser = (User)accountCreate;

        newUser.PasswordHash = PasswordHasher.Hash(accountCreate.Password); ;

        newUser.ProfilePicture = $"{_fileService.ImageFolderName}/default.jpg";

        newUser.Created = TimeHelper.GetCurrentDateTime();
        newUser.Updated = TimeHelper.GetCurrentDateTime();
        
        await _unitOfWork.Users.AddAsync(newUser);

        var email = new SendToEmailDTO();
        email.Email = accountCreate.Email;

        await SendCodeAsync(email);

        return true;
    }

    public async Task SendCodeAsync(SendToEmailDTO sendToEmail)
    {
        int code = new Random().Next(10000, 99999);

        _cache.Set(sendToEmail.Email, code, TimeSpan.FromMinutes(10));

        var message = new EmailMessage()
        {
            To = sendToEmail.Email,
            Subject = "Verifcation code",
            Body = code.ToString()
        };

        await _emailService.SendAsync(message);
    }

    public async Task<bool> VerifyEmailAsync(AccountEmailVerify verifyEmail)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(verifyEmail.Email);

        if (user is null)
            throw new StatusCodeException(HttpStatusCode.NotFound, message: "User not found");

        if (_cache.TryGetValue(verifyEmail.Email, out int expectedCode) is false)
            throw new StatusCodeException(HttpStatusCode.BadRequest, message: "Code is expired");

        if (expectedCode != verifyEmail.Code)
            throw new StatusCodeException(HttpStatusCode.BadRequest, message: "Code is wrong");

        user.IsEmailVerified = true;
        await _unitOfWork.Users.UpdateAsync(user.Id, user);
        return true;
    }

    public async Task<bool> VerifyPasswordAsync(AccountResetPasswordDTO userResetPassword)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(userResetPassword.Email);

        if (user is null)
            throw new StatusCodeException(HttpStatusCode.NotFound, message: "User not found");

        if (user.IsEmailVerified is false)
            throw new StatusCodeException(HttpStatusCode.BadRequest, message: "Email did not verified");

        user.PasswordHash = PasswordHasher.Hash(userResetPassword.Password);

        await _unitOfWork.Users.UpdateAsync(user.Id, user);

        return true;
    }
}

