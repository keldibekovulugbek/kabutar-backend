using Kabutar.Domain.Attributes;
using System.ComponentModel.DataAnnotations;


namespace Kabutar.Service.DTOs.Accounts;

public record SendToEmailDTO
{
    [Required(ErrorMessage = "Email is required")]
    [Email]
    public string Email { get; set; } = string.Empty;
}
