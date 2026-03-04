using Kabutar.DataAccess.Interfaces;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Common;
using Kabutar.Service.DTOs.Users;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Users;
using System.Net;

namespace Kabutar.Service.Services.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public UserService(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        user.IsEmailVerified = false;
        await _unitOfWork.Users.DeleteAsync(user);
        return true;
    }

    public async Task<IEnumerable<UserViewModel>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.Select(u => (UserViewModel)u);
    }

    public async Task<UserViewModel> GetIdAsync(long id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        return (UserViewModel)user;
    }

    public async Task<UserViewModel> GetUsernameAsync(string username)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username.Trim())
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        return (UserViewModel)user;
    }

    public async Task<bool> ImageUpdateAsync(long id, AccountImageUploadDTO dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        if (!string.IsNullOrEmpty(user.ProfilePicture))
            await _fileService.DeleteImageAsync(user.ProfilePicture);

        user.ProfilePicture = await _fileService.SaveImageAsync(dto.Image);
        user.Updated = TimeHelper.GetCurrentDateTime();

        await _unitOfWork.Users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdateAsync(long id, UserUpdateDTO dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        var duplicate = await _unitOfWork.Users.GetByUsernameAsync(dto.Username.Trim());
        if (duplicate is not null && duplicate.Id != user.Id)
            throw new StatusCodeException(HttpStatusCode.BadRequest, "This username already exists");

        user.FirstName = dto.Firstname;
        user.LastName = dto.Lastname;
        user.Username = dto.Username;
        user.About = dto.About;
        user.Updated = TimeHelper.GetCurrentDateTime();

        await _unitOfWork.Users.UpdateAsync(user);
        return true;
    }

    public async Task UpdateLastActiveAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.LastActive = TimeHelper.GetCurrentDateTime();
            await _unitOfWork.Users.UpdateAsync(user);
        }
    }

    public async Task<UserSettingsViewModel> GetSettingsAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        var settings = await _unitOfWork.UserSettings.GetByUserIdAsync(userId);

        // Create default settings if not exists
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = "light",
                FontSize = "medium",
                ChatBackgroundImage = null
            };
            await _unitOfWork.UserSettings.AddAsync(settings);
        }

        return new UserSettingsViewModel
        {
            Id = settings.Id,
            UserId = settings.UserId,
            Theme = settings.Theme,
            ChatBackgroundImage = settings.ChatBackgroundImage,
            FontSize = settings.FontSize
        };
    }

    public async Task<UserSettingsViewModel> UpdateSettingsAsync(long userId, UserSettingsUpdateDTO dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        var settings = await _unitOfWork.UserSettings.GetByUserIdAsync(userId);

        // Create settings if not exists
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = dto.Theme ?? "light",
                FontSize = dto.FontSize ?? "medium",
                ChatBackgroundImage = dto.ChatBackgroundImage
            };
            await _unitOfWork.UserSettings.AddAsync(settings);
        }
        else
        {
            // Update only non-null values
            if (dto.Theme != null)
                settings.Theme = dto.Theme;
            if (dto.FontSize != null)
                settings.FontSize = dto.FontSize;
            if (dto.ChatBackgroundImage != null)
                settings.ChatBackgroundImage = dto.ChatBackgroundImage;

            settings.Updated = TimeHelper.GetCurrentDateTime();
            await _unitOfWork.UserSettings.UpdateAsync(settings);
        }

        return new UserSettingsViewModel
        {
            Id = settings.Id,
            UserId = settings.UserId,
            Theme = settings.Theme,
            ChatBackgroundImage = settings.ChatBackgroundImage,
            FontSize = settings.FontSize
        };
    }

    public async Task<bool> UploadChatBackgroundAsync(long userId, AccountImageUploadDTO dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new StatusCodeException(HttpStatusCode.NotFound, "User does not exist");

        var settings = await _unitOfWork.UserSettings.GetByUserIdAsync(userId);

        // Create settings if not exists
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = "light",
                FontSize = "medium"
            };
        }

        // Delete old background if exists
        if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
            await _fileService.DeleteImageAsync(settings.ChatBackgroundImage);

        // Save new background
        settings.ChatBackgroundImage = await _fileService.SaveImageAsync(dto.Image);
        settings.Updated = TimeHelper.GetCurrentDateTime();

        if (settings.Id == 0)
            await _unitOfWork.UserSettings.AddAsync(settings);
        else
            await _unitOfWork.UserSettings.UpdateAsync(settings);

        return true;
    }
}
