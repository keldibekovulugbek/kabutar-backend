using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record LoginDTO
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
