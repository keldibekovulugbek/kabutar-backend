using Kabutar.Domain.Common;

namespace Kabutar.Domain.Entities.Users;

public class UserSettings : Auditable
{
    public long UserId { get; set; }

    /// <summary>
    /// Theme: "light" or "dark"
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// Chat background image path (null = default)
    /// </summary>
    public string? ChatBackgroundImage { get; set; }

    /// <summary>
    /// Font size: "small", "medium", "large"
    /// </summary>
    public string FontSize { get; set; } = "medium";

    // Navigation property
    public virtual User User { get; set; } = null!;
}
