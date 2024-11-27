using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record AccountImageUploadDTO
{
    [Required]
    [DataType(DataType.Upload)]
    public string Image { get; set; } = string.Empty;
}
