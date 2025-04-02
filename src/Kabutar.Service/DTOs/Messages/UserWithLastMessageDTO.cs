namespace Kabutar.Service.DTOs.Messages;

public class UserWithLastMessageDTO
{
    public long UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string LastMessage { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}
