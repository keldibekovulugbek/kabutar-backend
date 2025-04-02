using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Users;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == email.ToLower().Trim());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
    }

    public async Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user is null) return false;

        user.PasswordHash = newPasswordHash;
        user.Updated = DateTime.UtcNow;

        _dbSet.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
