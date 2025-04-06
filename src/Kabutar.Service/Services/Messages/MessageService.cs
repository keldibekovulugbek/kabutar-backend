using Kabutar.Domain.Entities.Messages;
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

    public MessageService(
        IUnitOfWork unitOfWork,
        IIdentityHelperService identity,
        IFileService fileService,
        IChatNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _fileService = fileService;
        _notifier = notifier;
    }

    public async Task<bool> SendMessageAsync(MessageCreateDTO dto)
    {
        var senderId = _identity.GetUserId()
            ?? throw new StatusCodeException(HttpStatusCode.Unauthorized, "User not authorized");

        var message = new Message
        {
            Content = dto.Content,
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Created = TimeHelper.GetCurrentDateTime(),
            Updated = TimeHelper.GetCurrentDateTime()
        };

        if (dto.Attachment is not null)
        {
            var category = DetectFileCategory(dto.Attachment.FileName);
            var filePath = await _fileService.SaveAsync(dto.Attachment, category);
            message.Content += $" [file: {filePath}]";
        }

        await _unitOfWork.Messages.AddAsync(message);

        // Real-time notify
        await _notifier.SendMessageToUserAsync(dto.ReceiverId, new
        {
            SenderId = senderId,
            Content = message.Content,
            SentAt = message.Created
        });

        return true;
    }

    public async Task<IEnumerable<MessageViewModel>> GetConversationAsync(long userId1, long userId2)
    {
        var messages = await _unitOfWork.Messages.GetMessagesBetweenUsersAsync(userId1, userId2);
        return messages.Select(message => (MessageViewModel)message);
    }

    public async Task<IEnumerable<MessageViewModel>> GetUnreadMessagesAsync(long userId)
    {
        var messages = await _unitOfWork.Messages.GetUnreadMessagesForUserAsync(userId);
        return messages.Select(m => (MessageViewModel)m);
    }

    public async Task<bool> MarkAsReadAsync(long messageId)
    {
        await _unitOfWork.Messages.MarkMessageAsReadAsync(messageId);
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
            LastMessage = res.LastMessage?.Content ?? "",
            Timestamp = res.LastMessage?.Created ?? DateTime.MinValue
        });
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
            _ => FileCategory.Document // default
        };
    }
}
