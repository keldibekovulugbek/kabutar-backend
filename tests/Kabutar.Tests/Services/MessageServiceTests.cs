using FluentAssertions;
using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Attachments;
using Kabutar.Domain.Entities.Messages;
using Kabutar.Service.DTOs.Messages;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Services.Messages;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace Kabutar.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IIdentityHelperService> _identity;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<IChatNotifier> _notifier;
    private readonly Mock<IEncryptionService> _encryption;
    private readonly Mock<IOnlineTracker> _onlineTracker;
    private readonly Mock<IMessageRepository> _messageRepo;
    private readonly Mock<IAttachmentRepository> _attachmentRepo;
    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _identity = new Mock<IIdentityHelperService>();
        _fileService = new Mock<IFileService>();
        _notifier = new Mock<IChatNotifier>();
        _encryption = new Mock<IEncryptionService>();
        _onlineTracker = new Mock<IOnlineTracker>();
        _messageRepo = new Mock<IMessageRepository>();
        _attachmentRepo = new Mock<IAttachmentRepository>();

        _unitOfWork.Setup(u => u.Messages).Returns(_messageRepo.Object);
        _unitOfWork.Setup(u => u.Attachments).Returns(_attachmentRepo.Object);
        _encryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => $"enc:{s}");
        _encryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.StartsWith("enc:") ? s[4..] : s);
        _notifier.Setup(n => n.SendMessageToUserAsync(It.IsAny<long>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        _messageRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        _sut = new MessageService(
            _unitOfWork.Object,
            _identity.Object,
            _fileService.Object,
            _notifier.Object,
            _encryption.Object,
            _onlineTracker.Object);
    }

    [Fact]
    public async Task SendMessage_ValidText_ReturnsTrue()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "Salom" };

        var result = await _sut.SendMessageAsync(dto);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.AddAsync(It.IsAny<Message>()), Times.Once);
        _notifier.Verify(n => n.SendMessageToUserAsync(2, It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_NotAuthenticated_ThrowsUnauthorized()
    {
        _identity.Setup(i => i.GetUserId()).Returns((long?)null);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "Salom" };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendMessage_EmptyContent_NoAttachment_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "   " };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_ContentTooLong_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = new string('a', 4001) };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_ExactlyMaxLength_Succeeds()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = new string('x', 4000) };

        var result = await _sut.SendMessageAsync(dto);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendMessage_ToSelf_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 1, Content = "Salom" };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_AttachmentTooLarge_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(21L * 1024 * 1024);
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "", Attachment = fileMock.Object };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest)
            .WithMessage("*20 MB*");
    }

    [Fact]
    public async Task SendMessage_DisallowedExtension_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024L);
        fileMock.Setup(f => f.FileName).Returns("virus.exe");
        fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");

        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "", Attachment = fileMock.Object };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_DisallowedMimeType_ThrowsBadRequest()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024L);
        fileMock.Setup(f => f.FileName).Returns("script.jpg");
        fileMock.Setup(f => f.ContentType).Returns("text/html");

        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "", Attachment = fileMock.Object };

        var act = () => _sut.SendMessageAsync(dto);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_EncryptsContent()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var dto = new MessageCreateDTO { ReceiverId = 2, Content = "Maxfiy xabar" };
        Message? savedMessage = null;
        _messageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Callback<Message>(m => savedMessage = m)
            .Returns(Task.CompletedTask);

        await _sut.SendMessageAsync(dto);

        savedMessage!.Content.Should().Be("enc:Maxfiy xabar");
        savedMessage.Content.Should().NotBe("Maxfiy xabar");
    }

    [Fact]
    public async Task DeleteMessage_NotFound_ThrowsNotFound()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        _messageRepo.Setup(r => r.GetByIdAsync(99L, It.IsAny<bool>())).ReturnsAsync((Message?)null);

        var act = () => _sut.DeleteMessageAsync(99L);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMessage_NotOwner_ThrowsForbidden()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var msg = new Message { Id = 5, SenderId = 3, ReceiverId = 4 };
        _messageRepo.Setup(r => r.GetByIdAsync(5L, It.IsAny<bool>())).ReturnsAsync(msg);

        var act = () => _sut.DeleteMessageAsync(5L);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteMessage_BySender_CallsDeleteForUser()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var msg = new Message { Id = 5, SenderId = 1, ReceiverId = 2 };
        _messageRepo.Setup(r => r.GetByIdAsync(5L, It.IsAny<bool>())).ReturnsAsync(msg);
        _messageRepo.Setup(r => r.DeleteMessageForUserAsync(5L, 1L, true)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteMessageAsync(5L, deleteForBoth: false);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.DeleteMessageForUserAsync(5L, 1L, true), Times.Once);
        _messageRepo.Verify(r => r.DeleteMessageForBothAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMessage_ForBothBySender_CallsDeleteForBoth()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        var msg = new Message { Id = 5, SenderId = 1, ReceiverId = 2 };
        _messageRepo.Setup(r => r.GetByIdAsync(5L, It.IsAny<bool>())).ReturnsAsync(msg);
        _messageRepo.Setup(r => r.DeleteMessageForBothAsync(5L)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteMessageAsync(5L, deleteForBoth: true);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.DeleteMessageForBothAsync(5L), Times.Once);
    }

    [Fact]
    public async Task DeleteMessage_ForBothByReceiver_CallsDeleteForUserNotBoth()
    {
        _identity.Setup(i => i.GetUserId()).Returns(2L);
        var msg = new Message { Id = 5, SenderId = 1, ReceiverId = 2 };
        _messageRepo.Setup(r => r.GetByIdAsync(5L, It.IsAny<bool>())).ReturnsAsync(msg);
        _messageRepo.Setup(r => r.DeleteMessageForUserAsync(5L, 2L, false)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteMessageAsync(5L, deleteForBoth: true);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.DeleteMessageForBothAsync(It.IsAny<long>()), Times.Never);
        _messageRepo.Verify(r => r.DeleteMessageForUserAsync(5L, 2L, false), Times.Once);
    }

    [Fact]
    public async Task GetConversation_DecryptsMessages()
    {
        var messages = new List<Message>
        {
            new() { Id = 1, SenderId = 1, ReceiverId = 2, Content = "enc:Salom" },
            new() { Id = 2, SenderId = 2, ReceiverId = 1, Content = "enc:Qalay" }
        };
        _messageRepo.Setup(r => r.GetMessagesBetweenUsersAsync(1, 2, 1, 50))
            .ReturnsAsync(messages);

        var result = (await _sut.GetConversationAsync(1, 2)).ToList();

        result.Should().HaveCount(2);
        result[0].Content.Should().Be("Salom");
        result[1].Content.Should().Be("Qalay");
    }

    [Fact]
    public async Task GetConversation_EmptyHistory_ReturnsEmpty()
    {
        _messageRepo.Setup(r => r.GetMessagesBetweenUsersAsync(1, 2, 1, 50))
            .ReturnsAsync(new List<Message>());

        var result = await _sut.GetConversationAsync(1, 2);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConversation_CorruptedEncryption_ReturnRawContent()
    {
        _encryption.Setup(e => e.Decrypt("corrupted")).Throws<Exception>();
        var messages = new List<Message>
        {
            new() { Id = 1, SenderId = 1, ReceiverId = 2, Content = "corrupted" }
        };
        _messageRepo.Setup(r => r.GetMessagesBetweenUsersAsync(1, 2, 1, 50))
            .ReturnsAsync(messages);

        var result = (await _sut.GetConversationAsync(1, 2)).ToList();

        result[0].Content.Should().Be("corrupted");
    }

    [Fact]
    public async Task ClearChat_ForMe_CallsClearForUser()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        _messageRepo.Setup(r => r.ClearChatForUserAsync(1L, 2L)).Returns(Task.CompletedTask);

        var result = await _sut.ClearChatAsync(2L, clearForBoth: false);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.ClearChatForUserAsync(1L, 2L), Times.Once);
        _messageRepo.Verify(r => r.ClearChatForBothAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task ClearChat_ForBoth_CallsClearForBoth()
    {
        _identity.Setup(i => i.GetUserId()).Returns(1L);
        _messageRepo.Setup(r => r.ClearChatForBothAsync(1L, 2L)).Returns(Task.CompletedTask);

        var result = await _sut.ClearChatAsync(2L, clearForBoth: true);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.ClearChatForBothAsync(1L, 2L), Times.Once);
    }

    [Fact]
    public async Task ClearChat_NotAuthenticated_ThrowsUnauthorized()
    {
        _identity.Setup(i => i.GetUserId()).Returns((long?)null);

        var act = () => _sut.ClearChatAsync(2L);

        await act.Should().ThrowAsync<StatusCodeException>()
            .Where(e => e.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkAsRead_CallsRepository()
    {
        _messageRepo.Setup(r => r.MarkMessageAsReadAsync(7L)).Returns(Task.CompletedTask);

        var result = await _sut.MarkAsReadAsync(7L);

        result.Should().BeTrue();
        _messageRepo.Verify(r => r.MarkMessageAsReadAsync(7L), Times.Once);
    }
}
