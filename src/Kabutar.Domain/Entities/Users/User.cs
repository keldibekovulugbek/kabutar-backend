using Kabutar.Domain.Attributes;
using Kabutar.Domain.Common;
using Kabutar.Domain.Entities.Messages;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kabutar.Domain.Entities.Users;

public class User : Auditable
{
    [Name]
    public string FirstName { get; set; } = string.Empty;

    [Name]
    public string LastName { get; set; } = string.Empty;

    [UsernameCheck]
    public string Username { get; set; } = string.Empty;

    [Email]
    public string Email { get; set; } = string.Empty;

    [AllowedFiles(new[] { ".jpg", ".jpeg", ".png" })]
    public string ProfilePicture { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string? About { get; set; }

    public DateTime? LastActive { get; set; }

    public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();

    public virtual ICollection<Message> ReceivedMessages { get; set; } = new HashSet<Message>();
}
