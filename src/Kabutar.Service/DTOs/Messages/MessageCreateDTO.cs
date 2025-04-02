using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Messages;

public class MessageCreateDTO
{
    [Required]
    public long ReceiverId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public IFormFile? Attachment { get; set; }
}
