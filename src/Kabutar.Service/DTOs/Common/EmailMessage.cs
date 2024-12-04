

using Kabutar.Domain.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Common;

public class EmailMessage
{
    [Required]
    [Email]
    public string To { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}
