namespace UserManagementService;

/// <summary>
/// Основна модель користувача (з UsersService)
/// </summary>
public class UserProfile
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, User, Premium
    public string EnglishLevel { get; set; } = "A1"; // A1, A2, B1, B2, C1, C2
    public string PreferredAIModel { get; set; } = "GPT-3.5";
    public int DailyGoal { get; set; } = 10;
    public bool NotificationsEnabled { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Налаштування користувача (1:1 зв'язок з UserProfile)
/// </summary>
public class UserSettings
{
    public int SettingsId { get; set; }
    public int UserId { get; set; } // FK до UserProfiles
    public string Theme { get; set; } = "Light"; // Light, Dark, Auto
    public string Language { get; set; } = "en"; // en, uk, es, fr
    public bool NotificationsEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public string TimeZone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Підписки користувача (1:N зв'язок з UserProfile)
/// </summary>
public class UserSubscription
{
    public int SubscriptionId { get; set; }
    public int UserId { get; set; } // FK до UserProfiles
    public string PlanType { get; set; } = string.Empty; // Free, Premium, Pro, Enterprise
    public decimal Price { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoRenew { get; set; } = false;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Навички (довідник для M:N зв'язку)
/// </summary>
public class Skill
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Grammar, Vocabulary, Speaking, Listening, Reading, Writing
    public string Description { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Рівні навичок користувача (M:N зв'язок між UserProfile та Skill)
/// </summary>
public class UserSkillLevel
{
    public int UserId { get; set; } // FK до UserProfiles
    public int SkillId { get; set; } // FK до Skills
    public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced, Expert
    public int Progress { get; set; } = 0; // 0-100%
    public DateTime LastAssessed { get; set; }
    public DateTime? NextAssessment { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Розширена інформація про навички користувача (для запитів з JOIN)
/// </summary>
public class UserSkillInfo
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int? Progress { get; set; }
    public DateTime LastAssessed { get; set; }
    public DateTime? NextAssessment { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Користувач зі статистикою (для звітів)
/// </summary>
public class UserWithStatistics
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EnglishLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // З UserSettings
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool NotificationsEnabled { get; set; }

    // Статистика
    public int ActiveSubscriptions { get; set; }
    public int SkillsCount { get; set; }
}

/// <summary>
/// Статистика навичок по категоріях
/// </summary>
public class SkillCategoryStatistics
{
    public string Category { get; set; } = string.Empty;
    public int TotalSkills { get; set; }
    public int UsersWithSkills { get; set; }
    public decimal AverageLevel { get; set; }
}

/// <summary>
/// Статистика користувачів (з старого UsersService)
/// </summary>
public class UserStatistic
{
    public string Role { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
}