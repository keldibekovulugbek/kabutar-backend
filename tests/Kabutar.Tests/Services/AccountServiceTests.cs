using FluentAssertions;
using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.DTOs.Accounts;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Security;
using Kabutar.Service.Services.Accounts;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Net;

namespace Kabutar.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IAuthManager> _authManager;
    private readonly Mock<IEmailService> _emailService;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly IMemoryCache _cache;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _authManager = new Mock<IAuthManager>();
        _emailService = new Mock<IEmailService>();
        _fileService = new Mock<IFileService>();
        _userRepo = new Mock<IUserRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _unitOfWork.Setup(u => u.Users).Returns(_userRepo.Object);
        _emailService.Setup(e => e.SendAsync(It.IsAny<Kabutar.Service.DTOs.Common.EmailMessage>()))
            .Returns(Task.CompletedTask);

        _sut = new AccountService(
            _unitOfWork.Object,
            _authManager.Object,
            _cache,
            _emailService.Object,
            _fileService.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        var user = new User { Id = 1, Email = "test@test.com", Username = "test", PasswordHash = hash, IsEmailVerified = true };
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _authManager.Setup(a => a.GenerateToken(user)).Returns("jwt.token.here");

        var result = await _sut.LogInAsync(new LoginDTO { UsernameOrEmail = "test@test.com", Password = "Parol123!" });

        result.Should().Be("jwt.token.here");
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = () => _sut.LogInAsync(new LoginDTO { UsernameOrEmail = "notexist", Password = "Parol123!" });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_EmailNotVerified_ThrowsBadRequest()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = hash, IsEmailVerified = false };
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var act = () => _sut.LogInAsync(new LoginDTO { UsernameOrEmail = "test@test.com", Password = "Parol123!" });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*not verified*");
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsBadRequest()
    {
        var hash = PasswordHasher.Hash("TogriBol123!");
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = hash, IsEmailVerified = true };
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var act = () => _sut.LogInAsync(new LoginDTO { UsernameOrEmail = "test@test.com", Password = "XatoPaol!1" });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*password*");
    }

    [Fact]
    public async Task Login_ByUsername_Succeeds()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        var user = new User { Id = 2, Username = "ulugbek", PasswordHash = hash, IsEmailVerified = true };
        _userRepo.Setup(r => r.GetByUsernameAsync("ulugbek")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _authManager.Setup(a => a.GenerateToken(user)).Returns("token");

        var result = await _sut.LogInAsync(new LoginDTO { UsernameOrEmail = "ulugbek", Password = "Parol123!" });

        result.Should().Be("token");
    }

    [Fact]
    public async Task Register_NewUser_Succeeds()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByUsernameAsync("newuser")).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var dto = new RegisterDTO
        {
            Firstname = "Ulugbek",
            Lastname = "Keldi",
            Email = "new@test.com",
            Username = "newuser",
            Password = "Parol123!"
        };

        var result = await _sut.RegisterAsync(dto);

        result.Should().BeTrue();
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _emailService.Verify(e => e.SendAsync(It.IsAny<Kabutar.Service.DTOs.Common.EmailMessage>()), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsBadRequest()
    {
        var existing = new User { Email = "dup@test.com", IsDeleted = false };
        _userRepo.Setup(r => r.GetByEmailAsync("dup@test.com")).ReturnsAsync(existing);

        var dto = new RegisterDTO
        {
            Firstname = "Test", Lastname = "User",
            Email = "dup@test.com", Username = "newuser2", Password = "Parol123!"
        };

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*Email already exists*");
    }

    [Fact]
    public async Task Register_DuplicateUsername_ThrowsBadRequest()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var existing = new User { Username = "taken", IsDeleted = false };
        _userRepo.Setup(r => r.GetByUsernameAsync("taken")).ReturnsAsync(existing);

        var dto = new RegisterDTO
        {
            Firstname = "Test", Lastname = "User",
            Email = "fresh@test.com", Username = "taken", Password = "Parol123!"
        };

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*Username already exists*");
    }

    [Fact]
    public async Task Register_PasswordIsHashed()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        User? savedUser = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u)
            .Returns(Task.CompletedTask);

        var dto = new RegisterDTO
        {
            Firstname = "Test", Lastname = "User",
            Email = "hash@test.com", Username = "hashuser", Password = "Parol123!"
        };

        await _sut.RegisterAsync(dto);

        savedUser!.PasswordHash.Should().NotBe("Parol123!");
        PasswordHasher.Verify("Parol123!", savedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmail_CorrectCode_ReturnsTrue()
    {
        var user = new User { Id = 1, Email = "v@test.com", IsEmailVerified = false };
        _userRepo.Setup(r => r.GetByEmailAsync("v@test.com")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _cache.Set("v@test.com", 12345);

        var result = await _sut.VerifyEmailAsync(new AccountEmailVerify { Email = "v@test.com", Code = 12345 });

        result.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmail_WrongCode_ThrowsBadRequest()
    {
        var user = new User { Id = 1, Email = "v@test.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("v@test.com")).ReturnsAsync(user);
        _cache.Set("v@test.com", 12345);

        var act = () => _sut.VerifyEmailAsync(new AccountEmailVerify { Email = "v@test.com", Code = 99999 });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*Incorrect*");
    }

    [Fact]
    public async Task VerifyEmail_ExpiredCode_ThrowsBadRequest()
    {
        var user = new User { Id = 1, Email = "expired@test.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("expired@test.com")).ReturnsAsync(user);

        var act = () => _sut.VerifyEmailAsync(new AccountEmailVerify { Email = "expired@test.com", Code = 12345 });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task VerifyEmail_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("ghost@test.com")).ReturnsAsync((User?)null);

        var act = () => _sut.VerifyEmailAsync(new AccountEmailVerify { Email = "ghost@test.com", Code = 11111 });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendCode_SendsEmail()
    {
        await _sut.SendCodeAsync(new SendToEmailDTO { Email = "send@test.com" });

        _emailService.Verify(e => e.SendAsync(It.Is<Kabutar.Service.DTOs.Common.EmailMessage>(
            m => m.To == "send@test.com")), Times.Once);
    }

    [Fact]
    public async Task SendCode_WhenBlocked_ThrowsTooManyRequests()
    {
        _cache.Set("block-blocked@test.com", true);

        var act = () => _sut.SendCodeAsync(new SendToEmailDTO { Email = "blocked@test.com" });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task SendCode_SetsBlockCacheAfterSend()
    {
        await _sut.SendCodeAsync(new SendToEmailDTO { Email = "block_after@test.com" });

        _cache.TryGetValue("block-block_after@test.com", out bool blocked).Should().BeTrue();
        blocked.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_CorrectCode_UpdatesHash()
    {
        var user = new User { Id = 1, Email = "reset@test.com", PasswordHash = "old" };
        _userRepo.Setup(r => r.GetByEmailAsync("reset@test.com")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _cache.Set("reset@test.com", 55555);

        var result = await _sut.ResetPasswordAsync(new AccountResetPasswordDTO
        {
            Email = "reset@test.com",
            Code = 55555,
            Password = "YangiParol1!"
        });

        result.Should().BeTrue();
        user.PasswordHash.Should().NotBe("old");
        PasswordHasher.Verify("YangiParol1!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_WrongCode_ThrowsBadRequest()
    {
        var user = new User { Id = 1, Email = "reset2@test.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("reset2@test.com")).ReturnsAsync(user);
        _cache.Set("reset2@test.com", 11111);

        var act = () => _sut.ResetPasswordAsync(new AccountResetPasswordDTO
        {
            Email = "reset2@test.com",
            Code = 99999,
            Password = "YangiParol1!"
        });

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }
}
