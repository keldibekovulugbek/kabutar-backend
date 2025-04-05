using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Kabutar.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AllowedFilesAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedFilesAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file is null) return ValidationResult.Success;

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_extensions.Contains(extension))
        {
            return new ValidationResult($"Invalid file type '{extension}'. Allowed extensions are: {string.Join(", ", _extensions)}");
        }

        return ValidationResult.Success;
    }
}
