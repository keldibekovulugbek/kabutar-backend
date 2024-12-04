using Kabutar.Domain.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record AccountImageUploadDTO
{
    [Required]
    [DataType(DataType.Upload)]
    [AllowedFiles(new string[] { ".jpg", ".jpeg", ".png" } )]
    public IFormFile Image { get; set; } = null!;
}
