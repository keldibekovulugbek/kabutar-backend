using FluentAssertions;
using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Users;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Services.Search;
using Moq;

namespace Kabutar.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IEncryptionService> _encryption;
    private readonly Mock<IOnlineTracker> _onlineTracker;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IMessageRepository> _messageRepo;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _encryption = new Mock<IEncryptionService>();
        _onlineTracker = new Mock<IOnlineTracker>();
        _userRepo = new Mock<IUserRepository>();
        _messageRepo = new Mock<IMessageRepository>();

        _unitOfWork.Setup(u => u.Users).Returns(_userRepo.Object);
        _unitOfWork.Setup(u => u.Messages).Returns(_messageRepo.Object);
        _encryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.StartsWith("enc:") ? s[4..] : s);
        _onlineTracker.Setup(o => o.IsOnline(It.IsAny<long>())).Returns(false);

        _sut = new SearchService(_unitOfWork.Object, _encryption.Object, _onlineTracker.Object);
    }

    [Fact]
    public async Task Search_EmptyText_ReturnsEmptyResult()
    {
        var result = await _sut.SearchAsync("   ", 1);

        result.Users.Should().BeEmpty();
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_NullText_ReturnsEmptyResult()
    {
        var result = await _sut.SearchAsync(null!, 1);

        result.Users.Should().BeEmpty();
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ValidText_ReturnsMatchingUsers()
    {
        var users = new List<User>
        {
            new() { Id = 5, Username = "ulugbek", FirstName = "Ulugbek", LastName = "Keldi" },
            new() { Id = 6, Username = "aziz", FirstName = "Aziz", LastName = "Karimov" }
        };
        _userRepo.Setup(r => r.SearchUsersAsync("ulug", 1L, 20)).ReturnsAsync(new List<User> { users[0] });
        _messageRepo.Setup(r => r.GetAllMessagesForUserAsync(1L, It.IsAny<int>())).ReturnsAsync(new List<Message>());

        var result = await _sut.SearchAsync("ulug", 1);

        result.Users.Should().HaveCount(1);
        result.Users[0].Username.Should().Be("ulugbek");
    }

    [Fact]
    public async Task Search_FindsDecryptedMessageContent()
    {
        var sender = new User { Id = 2, Username = "sender", FirstName = "S", LastName = "K" };
        var receiver = new User { Id = 1, Username = "me", FirstName = "M", LastName = "E" };

        var messages = new List<Message>
        {
            new()
            {
                Id = 10, SenderId = 2, ReceiverId = 1,
                Content = "enc:Salom do'stim",
                Sender = sender, Receiver = receiver,
                Created = DateTime.UtcNow
            }
        };
        _userRepo.Setup(r => r.SearchUsersAsync(It.IsAny<string>(), 1L, 20)).ReturnsAsync(new List<User>());
        _messageRepo.Setup(r => r.GetAllMessagesForUserAsync(1L, It.IsAny<int>())).ReturnsAsync(messages);

        var result = await _sut.SearchAsync("salom", 1);

        result.Messages.Should().HaveCount(1);
        result.Messages[0].MessageContent.Should().Be("Salom do'stim");
    }

    [Fact]
    public async Task Search_DoesNotReturnNonMatchingMessages()
    {
        var sender = new User { Id = 2, Username = "sender", FirstName = "S", LastName = "K" };
        var receiver = new User { Id = 1, Username = "me", FirstName = "M", LastName = "E" };
        var messages = new List<Message>
        {
            new()
            {
                Id = 11, SenderId = 2, ReceiverId = 1,
                Content = "enc:Qalay yashayapsiz",
                Sender = sender, Receiver = receiver,
                Created = DateTime.UtcNow
            }
        };
        _userRepo.Setup(r => r.SearchUsersAsync(It.IsAny<string>(), 1L, 20)).ReturnsAsync(new List<User>());
        _messageRepo.Setup(r => r.GetAllMessagesForUserAsync(1L, It.IsAny<int>())).ReturnsAsync(messages);

        var result = await _sut.SearchAsync("salom", 1);

        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_CorruptEncryption_StillReturnsRawContent()
    {
        _encryption.Setup(e => e.Decrypt("bad_cipher")).Throws<Exception>();
        var sender = new User { Id = 2, Username = "s", FirstName = "S", LastName = "K" };
        var receiver = new User { Id = 1, Username = "me", FirstName = "M", LastName = "E" };
        var messages = new List<Message>
        {
            new()
            {
                Id = 12, SenderId = 2, ReceiverId = 1,
                Content = "bad_cipher",
                Sender = sender, Receiver = receiver,
                Created = DateTime.UtcNow
            }
        };
        _userRepo.Setup(r => r.SearchUsersAsync(It.IsAny<string>(), 1L, 20)).ReturnsAsync(new List<User>());
        _messageRepo.Setup(r => r.GetAllMessagesForUserAsync(1L, It.IsAny<int>())).ReturnsAsync(messages);

        var result = await _sut.SearchAsync("bad_cipher", 1);

        result.Messages.Should().HaveCount(1);
        result.Messages[0].MessageContent.Should().Be("bad_cipher");
    }

    [Fact]
    public async Task Search_UsersShowsOnlineStatus()
    {
        var user = new User { Id = 7, Username = "online_user", FirstName = "O", LastName = "U" };
        _userRepo.Setup(r => r.SearchUsersAsync("online", 1L, 20)).ReturnsAsync(new List<User> { user });
        _messageRepo.Setup(r => r.GetAllMessagesForUserAsync(1L, It.IsAny<int>())).ReturnsAsync(new List<Message>());
        _onlineTracker.Setup(o => o.IsOnline(7L)).Returns(true);

        var result = await _sut.SearchAsync("online", 1);

        result.Users[0].IsOnline.Should().BeTrue();
    }
}
