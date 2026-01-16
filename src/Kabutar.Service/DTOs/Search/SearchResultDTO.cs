namespace Kabutar.Service.DTOs.Search;

public class SearchResultDTO
{
    public List<UserSearchResultDTO> Users { get; set; } = new();
    public List<MessageSearchResultDTO> Messages { get; set; } = new();
}

public class UserSearchResultDTO
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public bool IsOnline { get; set; }
}

public class MessageSearchResultDTO
{
    public long MessageId { get; set; }
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
