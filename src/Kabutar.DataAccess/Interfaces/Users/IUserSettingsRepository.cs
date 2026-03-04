using Kabutar.Domain.Entities.Users;

namespace Kabutar.DataAccess.Interfaces.Users;

public interface IUserSettingsRepository : IGenericRepository<UserSettings>
{
    Task<UserSettings?> GetByUserIdAsync(long userId);
}
