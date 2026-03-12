namespace Kabutar.Service.DTOs.Messages;

public class UserWithLastMessageDTO
{
    public long UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePicture { get; set; }

    public string? ProfilePictureThumbnail { get; set; }

    public string LastMessage { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public int UnreadCount { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastActive { get; set; }
}
