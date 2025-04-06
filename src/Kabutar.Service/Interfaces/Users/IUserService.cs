using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.DTOs.Users;
namespace Kabutar.Service.Interfaces.Users;

public interface IUserService
{
    Task<bool> UpdateAsync(long id, UserUpdateDTO viewModel);
    Task<bool> DeleteAsync(long id);
    Task<UserViewModel> GetIdAsync(long id);
    Task<UserViewModel> GetUsernameAsync(string username);
    Task<bool> ImageUpdateAsync(long id, AccountImageUploadDTO dto);
    Task<IEnumerable<UserViewModel>> GetAllAsync();
}
