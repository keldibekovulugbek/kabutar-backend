using Kabutar.Domain.Entities.Messages;

namespace Kabutar.Service.DTOs.Messages;

public class MessageViewModel
{
    public long Id { get; set; }

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public bool HasAttachment { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime Created { get; set; }

    public static implicit operator MessageViewModel(Message message)
    {
        return new MessageViewModel
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content!,
            IsRead = message.IsRead,
            HasAttachment = message.Attachment != null,
            AttachmentUrl = message.Attachment?.FilePath,
            Created = message.Created
        };
    }
}
