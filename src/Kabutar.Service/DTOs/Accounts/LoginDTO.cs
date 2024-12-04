using Kabutar.Domain.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record LoginDTO
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;
}
