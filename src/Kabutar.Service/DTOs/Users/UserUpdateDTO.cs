using Kabutar.Domain.Entities.Users;
using Kabutar.Domain.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Users;

public class UserUpdateDTO
{
    [Required]
    [Name]
    public string Firstname { get; set; } = string.Empty;

    [Required]
    [Name]
    public string Lastname { get; set; } = string.Empty;

    [Required]
    [UsernameCheck]
    public string Username { get; set; } = string.Empty;

    public string? About { get; set; }

    public static implicit operator User(UserUpdateDTO userUpdate)
    {
        return new User()
        {
            FirstName = userUpdate.Firstname,
            LastName = userUpdate.Lastname,
            Username = userUpdate.Username,
            About = userUpdate.About,
        };
    }
}
