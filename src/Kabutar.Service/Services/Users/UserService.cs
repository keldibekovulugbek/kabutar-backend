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
}
