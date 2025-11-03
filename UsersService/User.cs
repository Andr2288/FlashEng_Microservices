namespace UsersService;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserProfile
{
    public int ProfileId { get; set; }
    public int UserId { get; set; }
    public string EnglishLevel { get; set; }
    public string PreferredAIModel { get; set; }
    public int DailyGoal { get; set; }
    public bool NotificationsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EnglishLevel { get; set; }
    public string PreferredAIModel { get; set; }
    public int DailyGoal { get; set; }
    public bool NotificationsEnabled { get; set; }
    public string Roles { get; set; }
}