using Kabutar.Domain.Entities.Users;
using Kabutar.Service.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kabutar.Service.Security;

public class AuthManager : IAuthManager
{
    private readonly IConfigurationSection _configuration;

    public AuthManager(IConfiguration configuration)
    {
        _configuration = configuration.GetSection("Jwt");
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FirstName)
    };

        var key = Encoding.UTF8.GetBytes(_configuration["Key"]!);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            audience: _configuration["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Lifetime"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
