namespace Kabutar.Service.DTOs.Users;

public class UserSettingsDTO
{
    public string Theme { get; set; } = "light";
    public string? ChatBackgroundImage { get; set; }
    public string FontSize { get; set; } = "medium";
}

public class UserSettingsUpdateDTO
{
    public string? Theme { get; set; }
    public string? ChatBackgroundImage { get; set; }
    public string? FontSize { get; set; }
}

public class UserSettingsViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Theme { get; set; } = "light";
    public string? ChatBackgroundImage { get; set; }
    public string FontSize { get; set; } = "medium";
}
