﻿
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Accounts;

public record AccountResetPasswordDTO
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int Code { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
