namespace UserManagementService;

/// <summary>
/// Основна модель користувача
/// </summary>
public class UserProfile
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Налаштування користувача (1:1 зв'язок)
/// </summary>
public class UserSettings
{
    public int SettingsId { get; set; }
    public int UserId { get; set; }
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en";
    public bool NotificationsEnabled { get; set; } = true;
}