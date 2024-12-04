using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Kabutar.DataAccess.Repositories.Users
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly DbSet<User> _users;

        public UserRepository(AppDbContext context) : base(context)
        {
            _users = context.Set<User>();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _users.FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash)
        {
            var user = await _users.FindAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = newPasswordHash; 

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
