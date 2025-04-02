using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Kabutar.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class StrongPasswordAttribute : ValidationAttribute
{
    private const string _pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,50}$";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return new ValidationResult("Password cannot be null!");

        var password = value.ToString();
        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Password cannot be empty!");

        if (!Regex.IsMatch(password, _pattern))
            return new ValidationResult("Password must be 8-50 characters and include at least one lowercase, one uppercase, and one digit.");

        return ValidationResult.Success;
    }
}
