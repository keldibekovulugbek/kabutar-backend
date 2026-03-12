using Kabutar.Domain.Entities.Users;

namespace Kabutar.Service.DTOs.Users;

public record UserViewModel
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string? About { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastActive { get; set; }

    public static implicit operator UserViewModel(User user)
    {
        return new UserViewModel()
        {
            Id = user.Id,
            Fullname = $"{user.FirstName} {user.LastName}",
            Username = user.Username,
            About = user.About,
            ImagePath = user.ProfilePicture,
            ThumbnailPath = user.ProfilePictureThumbnail,
            CreatedAt = user.Created.ToString(),
            LastActive = user.LastActive,
            IsOnline = user.LastActive.HasValue && user.LastActive.Value > DateTime.UtcNow.AddMinutes(-5)
        };
    }
}
