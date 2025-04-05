using Kabutar.Domain.Entities.Users;

namespace Kabutar.Service.Interfaces.Common;

public interface IAuthManager
{
    public string GenerateToken(User user);
}
