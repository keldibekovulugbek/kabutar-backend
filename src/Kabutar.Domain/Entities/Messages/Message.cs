

using Kabutar.Domain.Common;
using Kabutar.Domain.Entities.Users;

namespace Kabutar.Domain.Entities.Messages; 

public class Message : Auditable
{
    public string Content { get; set; } = string.Empty;

    public long SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public long ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;

    public bool IsRead { get; set; } = false;
    
    public bool IsDeletedBySender { get; set; } = false;
    
    public bool IsDeletedByReceiver { get; set; } = false;
}
