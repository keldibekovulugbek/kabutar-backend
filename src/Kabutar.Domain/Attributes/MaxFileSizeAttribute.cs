using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxSizeMb;

    public MaxFileSizeAttribute(int maxSizeMb)
    {
        _maxSizeMb = maxSizeMb;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file is null) return ValidationResult.Success;

        var sizeInMb = file.Length / (1024.0 * 1024.0); // double type
        if (sizeInMb > _maxSizeMb)
            return new ValidationResult($"File size must be less than {_maxSizeMb} MB (Current: {Math.Round(sizeInMb, 2)} MB)");

        return ValidationResult.Success;
    }
}
