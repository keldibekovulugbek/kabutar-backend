using System.Threading.Tasks;
using Kabutar.Domain.Entities.Users;

namespace Kabutar.DataAccess.Interfaces.Users
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash);
    }
}
