using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Kabutar.DataAccess.Repositories.Users;

public class UserSettingsRepository : GenericRepository<UserSettings>, IUserSettingsRepository
{
    public UserSettingsRepository(AppDbContext context) : base(context) { }

    public async Task<UserSettings?> GetByUserIdAsync(long userId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
