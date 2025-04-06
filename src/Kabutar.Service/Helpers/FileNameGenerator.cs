using Kabutar.Domain.Enums;

namespace Kabutar.Service.Helpers;

public static class FileNameGenerator
{
    public static string Generate(string originalFileName, FileCategory category)
    {
        string extension = Path.GetExtension(originalFileName);
        string prefix = GetPrefix(category);
        string uniqueName = $"{prefix}_{Guid.NewGuid()}{extension}";

        return uniqueName;
    }

    private static string GetPrefix(FileCategory category)
    {
        return category switch
        {
            FileCategory.ProfilePicture => "PROFILE",
            FileCategory.MessageImage => "IMG",
            FileCategory.Video => "VID",
            FileCategory.Document => "DOC",
            FileCategory.Music => "MUSIC",
            FileCategory.VoiceMessage => "VOICE",
            _ => "FILE"
        };
    }
}
