
using Kabutar.Domain.Common;
using Kabutar.Domain.Entities.Attachments;
using Kabutar.Domain.Entities.Users;

namespace Kabutar.Domain.Entities.Messages;

public class Message : Auditable
{
    public string? Content { get; set; }

    public long SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;

    public long ReceiverId { get; set; }
    public virtual User Receiver { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public bool IsDeletedBySender { get; set; }
    public bool IsDeletedByReceiver { get; set; }

    public virtual Attachment? Attachment { get; set; }
}
