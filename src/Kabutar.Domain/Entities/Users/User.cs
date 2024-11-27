using Kabutar.Domain.Common;
using Kabutar.Domain.Entities.Messages;

namespace Kabutar.Domain.Entities.Users;

public class User : Auditable
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ProfilePicture { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public string About { get; set; } = string.Empty;

    public DateTime? LastActive { get; set; }

    public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();

    public virtual ICollection<Message> ReceivedMessages { get; set; } = new HashSet<Message>();

}
