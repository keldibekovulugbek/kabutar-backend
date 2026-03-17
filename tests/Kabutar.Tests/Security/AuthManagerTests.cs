using FluentAssertions;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.Security;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kabutar.Tests.Security;

public class AuthManagerTests
{
    private readonly AuthManager _sut;
    private readonly User _testUser = new()
    {
        Id = 42,
        FirstName = "Ulugbek",
        LastName = "Keldi",
        Email = "test@kabutar.com",
        Username = "ulugbek"
    };

    public AuthManagerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKey_AtLeast32Characters_ForHmac256",
                ["Jwt:Issuer"] = "https://kabutar.test",
                ["Jwt:Audience"] = "KabutarTest",
                ["Jwt:Lifetime"] = "60"
            })
            .Build();

        _sut = new AuthManager(config);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateToken(_testUser);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_IsValidJwt()
    {
        var token = _sut.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_ContainsUserId()
    {
        var token = _sut.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
            || c.Type == "nameid"
            || c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        sub.Should().Be("42");
    }

    [Fact]
    public void GenerateToken_ContainsEmail()
    {
        var token = _sut.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var email = jwt.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

        email.Should().Be("test@kabutar.com");
    }

    [Fact]
    public void GenerateToken_HasFutureExpiry()
    {
        var before = DateTime.UtcNow;
        var token = _sut.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeAfter(before);
    }

    [Fact]
    public void GenerateToken_TwoCalls_ProduceDifferentTokens()
    {
        var t1 = _sut.GenerateToken(_testUser);
        System.Threading.Thread.Sleep(1100);
        var t2 = _sut.GenerateToken(_testUser);

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuer()
    {
        var token = _sut.GenerateToken(_testUser);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("https://kabutar.test");
    }
}
