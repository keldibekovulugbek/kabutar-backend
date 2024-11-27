using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record AccountImageUploadDTO
{
    [Required]
    [DataType(DataType.Upload)]
    public IFormFile Image { get; set; } = null!;
}
