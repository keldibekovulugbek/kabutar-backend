using Kabutar.Domain.Entities.Users;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Service.DTOs.Users;

public class UserUpdateDTO
{
    [Required]
    public string Firstname { get; set; } = string.Empty;

    [Required]
    public string Lastname { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    public string? About { get; set; }

    public static implicit operator User(UserUpdateDTO userUpdate)
    {
        return new User()
        {
            FirstName = userUpdate.Firstname,
            LastName = userUpdate.Lastname,
            Username = userUpdate.Username,
            About = userUpdate.About,
        };
    }
}
