using Kabutar.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kabutar.Service.DTOs.Users;

public record UserViewModel
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string? About { get; set; } = string.Empty;

    public static implicit operator UserViewModel(User user)
    {
        return new UserViewModel()
        {
            Id = user.Id,
            Fullname = $"{user.FirstName} {user.LastName}",
            Username = user.Username,
            About = user.About,
            ImagePath = user.ProfilePicture,
            CreatedAt = user.Created.ToString()
        };
    }
}
