using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Attachments;
using Kabutar.Domain.Enums;
using Kabutar.Service.DTOs.Messages;
using Kabutar.Service.Exceptions;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces;
using System.Net;

namespace Kabutar.Service.Services.Messages;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityHelperService _identity;
    private readonly IFileService _fileService;
    private readonly IChatNotifier _notifier;
    private readonly IEncryptionService _encryption;
    private readonly IOnlineTracker _onlineTracker;

    public MessageService(
        IUnitOfWork unitOfWork,
        IIdentityHelperService identity,
        IFileService fileService,
        IChatNotifier notifier,
        IEncryptionService encryption,
        IOnlineTracker onlineTracker)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _fileService = fileService;
        _notifier = notifier;
        _encryption = encryption;
        _onlineTracker = onlineTracker;
    }

    public async Task<bool> SendMessageAsync(MessageCreateDTO dto)
    {
        var senderId = _identity.GetUserId()
            ?? throw new StatusCodeException(HttpStatusCode.Unauthorized, "User not authorized");

        var message = new Message
        {
            Content = _encryption.Encrypt(dto.Content),
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Created = TimeHelper.GetCurrentDateTime(),
            Updated = TimeHelper.GetCurrentDateTime()
        };

        await _unitOfWork.Messages.AddAsync(message);

        string? attachmentUrl = null;
        if (dto.Attachment is not null)
        {
            var category = DetectFileCategory(dto.Attachment.FileName);
            var filePath = await _fileService.SaveAsync(dto.Attachment, category);

            var attachment = new Attachment
            {
                MessageId = message.Id,
                FilePath = filePath,
                MimeType = dto.Attachment.ContentType,
                FileType = category.ToString(),
                Created = TimeHelper.GetCurrentDateTime(),
                Updated = TimeHelper.GetCurrentDateTime()
            };

            await _unitOfWork.Attachments.AddAsync(attachment);
            attachmentUrl = filePath;
        }

        await _notifier.SendMessageToUserAsync(dto.ReceiverId, new
        {
            SenderId = senderId,
            Content = dto.Content,
            SentAt = message.Created,
            AttachmentUrl = attachmentUrl
        });

        return true;
    }

    public async Task<IEnumerable<MessageViewModel>> GetConversationAsync(long userId1, long userId2)
    {
        var messages = await _unitOfWork.Messages.GetMessagesBetweenUsersAsync(userId1, userId2);
        return messages.Select(message =>
        {
            var vm = (MessageViewModel)message;
            vm.Content = DecryptSafe(vm.Content);
            return vm;
        });
    }

    public async Task<IEnumerable<MessageViewModel>> GetUnreadMessagesAsync(long userId)
    {
        var messages = await _unitOfWork.Messages.GetUnreadMessagesForUserAsync(userId);
        return messages.Select(m =>
        {
            var vm = (MessageViewModel)m;
            vm.Content = DecryptSafe(vm.Content);
            return vm;
        });
    }

    public async Task<bool> MarkAsReadAsync(long messageId)
    {
        await _unitOfWork.Messages.MarkMessageAsReadAsync(messageId);
        return true;
    }

    public async Task<bool> DeleteMessageAsync(long messageId, bool deleteForBoth = false)
    {
        var userId = _identity.GetUserId()
            ?? throw new StatusCodeException(HttpStatusCode.Unauthorized, "User not authorized");

        var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
        if (message == null)
            throw new StatusCodeException(HttpStatusCode.NotFound, "Message not found");

        bool isSender = message.SenderId == userId;
        bool isReceiver = message.ReceiverId == userId;

        if (!isSender && !isReceiver)
            throw new StatusCodeException(HttpStatusCode.Forbidden, "Access denied");

        if (deleteForBoth && isSender)
        {
            await _unitOfWork.Messages.DeleteMessageForBothAsync(messageId);
        }
        else
        {
            await _unitOfWork.Messages.DeleteMessageForUserAsync(messageId, userId, isSender);
        }
        return true;
    }

    public async Task<bool> ClearChatAsync(long otherUserId, bool clearForBoth = false)
    {
        var userId = _identity.GetUserId()
            ?? throw new StatusCodeException(HttpStatusCode.Unauthorized, "User not authorized");

        if (clearForBoth)
            await _unitOfWork.Messages.ClearChatForBothAsync(userId, otherUserId);
        else
            await _unitOfWork.Messages.ClearChatForUserAsync(userId, otherUserId);

        return true;
    }

    public async Task<IEnumerable<UserWithLastMessageDTO>> GetAllChatUsersAsync(long userId)
    {
        var rawResults = await _unitOfWork.Messages
            .GetAllUsersAndLastMessagesWithOneUserAsync(userId);

        return rawResults.Select(res => new UserWithLastMessageDTO
        {
            UserId = res.User.Id,
            Username = res.User.Username,
            FirstName = res.User.FirstName,
            LastName = res.User.LastName,
            ProfilePicture = res.User.ProfilePicture,
            ProfilePictureThumbnail = res.User.ProfilePictureThumbnail,
            LastMessage = res.LastMessage != null ? DecryptSafe(res.LastMessage.Content) : "",
            Timestamp = res.LastMessage?.Created ?? DateTime.MinValue,
            UnreadCount = res.UnreadCount,
            IsOnline = _onlineTracker.IsOnline(res.User.Id),
            LastActive = res.User.LastActive
        });
    }
    private string DecryptSafe(string? content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        try
        {
            return _encryption.Decrypt(content);
        }
        catch
        {

            return content;
        }
    }

    private FileCategory DetectFileCategory(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();

        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" => FileCategory.MessageImage,
            ".pdf" => FileCategory.Document,
            ".doc" or ".docx" => FileCategory.Document,
            ".mp4" or ".avi" => FileCategory.Video,
            ".mp3" => FileCategory.Music,
            ".wav" => FileCategory.VoiceMessage,
            _ => FileCategory.Document
        };
    }
}
