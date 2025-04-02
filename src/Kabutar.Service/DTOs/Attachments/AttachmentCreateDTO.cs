using Microsoft.AspNetCore.Http;
using Kabutar.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Attachments;

public class AttachmentCreateDTO
{
    [Required]
    public long MessageId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public FileCategory Category { get; set; }
}
