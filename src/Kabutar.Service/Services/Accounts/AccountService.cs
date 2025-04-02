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
    private static readonly Random _random = new();

    public AccountService(
        IUnitOfWork unitOfWork,
        IAuthManager authManager,
        IMemoryCache cache,
        IEmailService emailService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _authManager = authManager;
        _cache = cache;
        _emailService = emailService;
        _fileService = fileService;
    }

    public async Task<string> LogInAsync(LoginDTO dto)
    {
        var user = await GetUserAndValidateCredentialsAsync(dto.UsernameOrEmail, dto.Password);
        return _authManager.GenerateToken(user);
    }

    public async Task<bool> RegisterAsync(RegisterDTO dto)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email.ToLower());
        if (existingUser is not null && !existingUser.IsDeleted && existingUser.IsEmailVerified)
            throw new StatusCodeException(HttpStatusCode.BadRequest, "User already exists.");

        var user = (User)dto;

        user.PasswordHash = PasswordHasher.Hash(dto.Password);
        user.ProfilePicture = $"{_fileService.ImageFolderName}/default.jpg";
        user.Created = TimeHelper.GetCurrentDateTime();
        user.Updated = TimeHelper.GetCurrentDateTime();

        await _unitOfWork.Users.AddAsync(user);
        await SendCodeAsync(new SendToEmailDTO { Email = user.Email });

        return true;
    }

    public async Task SendCodeAsync(SendToEmailDTO dto)
    {
        if (_cache.TryGetValue($"block-{dto.Email}", out _))
            throw new StatusCodeException(HttpStatusCode.TooManyRequests, "Too many requests. Please wait.");

        int code = _random.Next(10000, 99999);

        _cache.Set(dto.Email, code, TimeSpan.FromMinutes(10));
        _cache.Set($"block-{dto.Email}", true, TimeSpan.FromMinutes(1));

        var email = new EmailMessage
        {
            To = dto.Email,
            Subject = "Verification Code",
            Body = code.ToString()
        };

        await _emailService.SendAsync(email);
    }

    public async Task<bool> VerifyEmailAsync(AccountEmailVerify dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User not found.");

        if (!_cache.TryGetValue(dto.Email, out int expectedCode))
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Verification code expired.");

        if (dto.Code != expectedCode)
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Incorrect verification code.");

        user.IsEmailVerified = true;
        await _unitOfWork.Users.UpdateAsync(user);

        _cache.Remove(dto.Email);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(AccountResetPasswordDTO dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User not found.");

        if (!user.IsEmailVerified)
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Email not verified.");

        user.PasswordHash = PasswordHasher.Hash(dto.Password);
        await _unitOfWork.Users.UpdateAsync(user);

        return true;
    }

    private async Task<User> GetUserAndValidateCredentialsAsync(string input, string password)
    {
        User? user = input.Contains('@')
            ? await _unitOfWork.Users.GetByEmailAsync(input.ToLower().Trim())
            : await _unitOfWork.Users.GetByUsernameAsync(input);

        if (user is null)
            throw new StatusCodeException(HttpStatusCode.NotFound, "User not found.");

        if (!user.IsEmailVerified)
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Email not verified.");

        if (!PasswordHasher.Verify(password, user.PasswordHash))
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Incorrect password.");

        return user;
    }
}
