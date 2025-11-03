namespace UsersService;

/// <summary>
/// Спрощена модель користувача (замість Users + UserProfiles + Roles)
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
/// Модель замовлення (покупка категорій флешкарток)
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    public DateTime OrderDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

/// <summary>
/// Спрощена модель для показу даних користувача
/// </summary>
public class UserInfo
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EnglishLevel { get; set; } = string.Empty;
    public int DailyGoal { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}