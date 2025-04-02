using Kabutar.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Kabutar.Service.Interfaces.Common;

public interface IFileService
{
    string RootFolder { get; }
    string ImageFolderName { get; }

    Task<string> SaveImageAsync(IFormFile image);
    Task<string> SaveAsync(IFormFile file, FileCategory category);

    Task<bool> DeleteImageAsync(string relativeFilePath);
}
