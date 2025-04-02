using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Domain.Enums;

namespace Kabutar.Service.Services.Common;

public class FileService : IFileService
{
    private readonly string _basePath;
    private readonly string _imageFolderName = "images";
    private readonly string _profileImagesFolderName = "profile";

    public string RootFolder => _basePath;
    public string ImageFolderName => Path.Combine(_imageFolderName, _profileImagesFolderName);

    public FileService(IWebHostEnvironment webHost)
    {
        _basePath = webHost.WebRootPath;
    }

    public async Task<string> SaveImageAsync(IFormFile image)
        => await SaveAsync(image, FileCategory.ProfilePicture);

    public async Task<string> SaveAsync(IFormFile file, FileCategory category)
    {
        if (file == null || file.Length == 0)
            return "";

        string folder = GetFolderPath(category);
        string saveDir = Path.Combine(_basePath, folder);

        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);

        string fileName = FileNameGenerator.Generate(file.FileName, category);
        string relativePath = Path.Combine(folder, fileName);
        string fullPath = Path.Combine(_basePath, relativePath);

        using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return relativePath.Replace("\\", "/");
    }

    public Task<bool> DeleteImageAsync(string relativeFilePath)
    {
        string fullPath = Path.Combine(_basePath, relativeFilePath);
        if (!File.Exists(fullPath)) return Task.FromResult(false);

        try
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string GetFolderPath(FileCategory category)
    {
        return category switch
        {
            FileCategory.ProfilePicture => "images/profile",
            FileCategory.MessageImage => "images/message",
            FileCategory.Document => "documents",
            FileCategory.Video => "videos",
            FileCategory.Music => "audio/music",
            FileCategory.VoiceMessage => "audio/voice",
            _ => "misc"
        };
    }
}
