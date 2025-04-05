using Kabutar.Domain.Entities.Users;
using Kabutar.Domain.Entities.Messages;

namespace Kabutar.Domain.DTOs.Messages;

public class UserWithLastMessageVM
{
    public User User { get; set; } = null!;
    public Message? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
