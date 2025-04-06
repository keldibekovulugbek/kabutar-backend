using Kabutar.Service.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace Kabutar.Service.Managers;

public class IdentityHelperService : IIdentityHelperService
{
    private readonly IHttpContextAccessor _accessor;

    public IdentityHelperService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public long? GetUserId()
    {
        var claim = _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && long.TryParse(claim.Value, out var id) ? id : null;
    }

    public string GetUserName()
    {
        return _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public string GetUserEmail()
    {
        return _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }
}
