

using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record AccountEmailVerify
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int Code { get; set; }
}
