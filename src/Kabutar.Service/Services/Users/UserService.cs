using Kabutar.DataAccess.Interfaces;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Common;
using Kabutar.Service.DTOs.Users;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kabutar.Service.Services.Users
{
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
            var result = await _unitOfWork.Users.GetByIdAsync(id);

            if (result is null)
                throw new StatusCodeException(HttpStatusCode.NotFound, message: "User don't exist");


            await _unitOfWork.Users.DeleteAsync(result);

            return result != null;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();

            var userViews = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userView = (UserViewModel)user;

                userViews.Add(userView);
            }

            return userViews;
        }

        public async Task<UserViewModel> GetUsernameAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username.Trim());

            if (user is null)
                throw new StatusCodeException(HttpStatusCode.NotFound, message: "User not found");

            var userView = (UserViewModel)user;

            return userView;
        }

        public async Task<UserViewModel> GetIdAsync(long id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user is null)
                throw new StatusCodeException(HttpStatusCode.NotFound, message: "User not found");

            var userView = (UserViewModel)user;

            return userView;
        }

        public async Task<bool> ImageUpdateAsync(long id, AccountImageUploadDTO dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user!.ProfilePicture is not null)
            {
                await _fileService.DeleteImageAsync(user.ProfilePicture);

                user.ProfilePicture = await _fileService.SaveImageAsync(dto.Image);
            }
            await _unitOfWork.Users.UpdateAsync(id, user);

            return true;
        }

        public async Task<bool> UpdateAsync(long id, UserUpdateDTO viewModel)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            var userName = await _unitOfWork.Users.GetByUsernameAsync(viewModel.Username.Trim());
            //var phoneNumber = await _unitOfWork.Users.GetByPhonNumberAsync(viewModel.PhoneNumber.Trim());

            if (user is null)
                throw new StatusCodeException(HttpStatusCode.NotFound, message: "User not found");

            if (userName is not null && userName.Username != user.Username)
                throw new StatusCodeException(HttpStatusCode.BadRequest, message: "This username already exist");

            //if (phoneNumber is not null && phoneNumber.PhoneNumber != user.PhoneNumber)
            //    throw new StatusCodeException(HttpStatusCode.BadRequest, message: "This phoneNumber already exist");

            var newUser = (User)viewModel;

            await _unitOfWork.Users.UpdateAsync(id, newUser);

            return true;
        }

       }
}
