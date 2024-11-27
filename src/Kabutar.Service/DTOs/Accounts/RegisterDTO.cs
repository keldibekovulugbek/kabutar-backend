

using Kabutar.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record RegisterDTO
{
    [Required, MinLength(3)]
    public string Firstname { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string Lastname { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;


    public static implicit operator User(RegisterDTO registerDTO)
    {
        return new User()
        {
            FirstName = registerDTO.Firstname,
            LastName = registerDTO.Lastname,
            Email = registerDTO.Email,
            Username = registerDTO.Username,
        };
    }

}
