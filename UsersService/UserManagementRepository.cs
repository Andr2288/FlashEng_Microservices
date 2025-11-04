using MySql.Data.MySqlClient;
using Dapper;
using System.Data;

namespace UserManagementService;

/// <summary>
/// Розширений репозиторій для управління користувачами з повною демонстрацією зв'язків
/// </summary>
public class UserManagementRepository
{
    private readonly string _connectionString;

    public UserManagementRepository()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    // ===================================
    // CRUD для UserProfiles (основна таблиця з UsersService)
    // ===================================

    /// <summary>
    /// Отримати всіх користувачів
    /// </summary>
    public async Task<List<UserProfile>> GetAllUsersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM UserProfiles ORDER BY CreatedAt DESC";

        var users = await connection.QueryAsync<UserProfile>(sql);
        return users.ToList();
    }

    /// <summary>
    /// Отримати користувача по ID
    /// </summary>
    public async Task<UserProfile?> GetUserByIdAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM UserProfiles WHERE UserId = @UserId";

        return await connection.QueryFirstOrDefaultAsync<UserProfile>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Отримати користувача по Email
    /// </summary>
    public async Task<UserProfile?> GetUserByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM UserProfiles WHERE Email = @Email";

        return await connection.QueryFirstOrDefaultAsync<UserProfile>(sql, new { Email = email });
    }

    /// <summary>
    /// Створити нового користувача
    /// </summary>
    public async Task<int> CreateUserAsync(string email, string password, string fullName, string englishLevel = "A1", string role = "User")
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, EnglishLevel, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Email, @Password, @FullName, @Role, @EnglishLevel, 1, NOW(), NOW());
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            Email = email,
            Password = password,
            FullName = fullName,
            Role = role,
            EnglishLevel = englishLevel
        });
    }

    /// <summary>
    /// Оновити профіль користувача
    /// </summary>
    public async Task<int> UpdateUserProfileAsync(int userId, string? englishLevel = null, int? dailyGoal = null, bool? notificationsEnabled = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        var setParts = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (englishLevel != null)
        {
            setParts.Add("EnglishLevel = @EnglishLevel");
            parameters.Add("EnglishLevel", englishLevel);
        }

        if (dailyGoal.HasValue)
        {
            setParts.Add("DailyGoal = @DailyGoal");
            parameters.Add("DailyGoal", dailyGoal.Value);
        }

        if (notificationsEnabled.HasValue)
        {
            setParts.Add("NotificationsEnabled = @NotificationsEnabled");
            parameters.Add("NotificationsEnabled", notificationsEnabled.Value);
        }

        if (!setParts.Any())
            return 0;

        setParts.Add("UpdatedAt = NOW()");

        string sql = $"UPDATE UserProfiles SET {string.Join(", ", setParts)} WHERE UserId = @UserId";

        return await connection.ExecuteAsync(sql, parameters);
    }

    /// <summary>
    /// Статистика користувачів
    /// </summary>
    public async Task<List<UserStatistic>> GetUserStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                Role, 
                COUNT(*) as UserCount,
                SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveUsers
            FROM UserProfiles 
            GROUP BY Role";

        var stats = await connection.QueryAsync<UserStatistic>(sql);
        return stats.ToList();
    }

    // ===================================
    // CRUD для UserSettings (1:1 зв'язок)
    // ===================================

    /// <summary>
    /// Створити налаштування користувача
    /// </summary>
    public async Task<int> CreateUserSettingsAsync(int userId, string theme = "Light", string language = "en", bool notificationsEnabled = true)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled)
            VALUES (@UserId, @Theme, @Language, @NotificationsEnabled);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            UserId = userId,
            Theme = theme,
            Language = language,
            NotificationsEnabled = notificationsEnabled
        });
    }

    /// <summary>
    /// Отримати налаштування користувача (1:1)
    /// </summary>
    public async Task<UserSettings?> GetUserSettingsAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM UserSettings WHERE UserId = @UserId";

        return await connection.QueryFirstOrDefaultAsync<UserSettings>(sql, new { UserId = userId });
    }

    // ===================================
    // CRUD для UserSubscriptions (1:N зв'язок)
    // ===================================

    /// <summary>
    /// Створити підписку користувача
    /// </summary>
    public async Task<int> CreateUserSubscriptionAsync(int userId, string planType, DateTime? endDate = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO UserSubscriptions (UserId, PlanType, StartDate, EndDate, IsActive)
            VALUES (@UserId, @PlanType, NOW(), @EndDate, TRUE);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            UserId = userId,
            PlanType = planType,
            EndDate = endDate
        });
    }

    /// <summary>
    /// Отримати активні підписки користувача (1:N)
    /// </summary>
    public async Task<List<UserSubscription>> GetUserSubscriptionsAsync(int userId, bool activeOnly = true)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM UserSubscriptions WHERE UserId = @UserId";
        if (activeOnly)
            sql += " AND IsActive = TRUE";
        sql += " ORDER BY StartDate DESC";

        var subscriptions = await connection.QueryAsync<UserSubscription>(sql, new { UserId = userId });
        return subscriptions.ToList();
    }

    // ===================================
    // CRUD для Skills та UserSkillLevels (M:N зв'язок)
    // ===================================

    /// <summary>
    /// Створити навичку
    /// </summary>
    public async Task<int> CreateSkillAsync(string skillName, string category)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO Skills (SkillName, Category)
            VALUES (@SkillName, @Category);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new { SkillName = skillName, Category = category });
    }

    /// <summary>
    /// Отримати всі навички
    /// </summary>
    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Skills ORDER BY Category, SkillName";

        var skills = await connection.QueryAsync<Skill>(sql);
        return skills.ToList();
    }

    /// <summary>
    /// Додати навичку користувачу (M:N)
    /// </summary>
    public async Task<bool> AddUserSkillAsync(int userId, int skillId, string level = "Beginner")
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO UserSkillLevels (UserId, SkillId, Level, LastAssessed)
            VALUES (@UserId, @SkillId, @Level, NOW())
            ON DUPLICATE KEY UPDATE Level = @Level, LastAssessed = NOW()";

        int rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, SkillId = skillId, Level = level });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Отримати навички користувача (M:N)
    /// </summary>
    public async Task<List<UserSkillInfo>> GetUserSkillsAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                s.SkillId,
                s.SkillName,
                s.Category,
                usk.Level,
                usk.Progress,
                usk.LastAssessed
            FROM UserSkillLevels usk
            JOIN Skills s ON usk.SkillId = s.SkillId
            WHERE usk.UserId = @UserId
            ORDER BY s.Category, s.SkillName";

        var skills = await connection.QueryAsync<UserSkillInfo>(sql, new { UserId = userId });
        return skills.ToList();
    }

    // ===================================
    // ЗБЕРЕЖУВАНІ ПРОЦЕДУРИ
    // ===================================

    /// <summary>
    /// Створити користувача з налаштуваннями через збережувану процедуру
    /// </summary>
    public async Task<int> CreateUserWithSettingsAsync(string email, string password, string fullName, string theme = "Light", string language = "en")
    {
        using var connection = new MySqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("p_Email", email);
        parameters.Add("p_Password", password);
        parameters.Add("p_FullName", fullName);
        parameters.Add("p_Theme", theme);
        parameters.Add("p_Language", language);
        parameters.Add("p_UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("CreateUserWithSettings", parameters, commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("p_UserId");
    }

    /// <summary>
    /// Отримати повний профіль користувача через збережувану процедуру
    /// </summary>
    public async Task<(UserProfile? profile, UserSettings? settings, List<UserSubscription> subscriptions, List<UserSkillInfo> skills)> GetUserFullProfileAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync("GetUserFullProfile",
            new { p_UserId = userId },
            commandType: CommandType.StoredProcedure);

        var profile = multi.Read<UserProfile>().FirstOrDefault();
        var subscriptions = multi.Read<UserSubscription>().ToList();
        var skills = multi.Read<UserSkillInfo>().ToList();

        // Settings отримуємо окремо через простий запит
        var settings = await GetUserSettingsAsync(userId);

        return (profile, settings, subscriptions, skills);
    }

    /// <summary>
    /// Оновити підписку користувача через збережувану процедуру
    /// </summary>
    public async Task<string> UpdateUserSubscriptionAsync(int userId, string planType, int durationMonths)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QuerySingleAsync<dynamic>("UpdateUserSubscription",
            new { p_UserId = userId, p_PlanType = planType, p_DurationMonths = durationMonths },
            commandType: CommandType.StoredProcedure);

        return result.Message;
    }

    /// <summary>
    /// Деактивувати користувача каскадно через збережувану процедуру
    /// </summary>
    public async Task<string> DeactivateUserCascadeAsync(int userId, string reason)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QuerySingleAsync<dynamic>("DeactivateUserCascade",
            new { p_UserId = userId, p_Reason = reason },
            commandType: CommandType.StoredProcedure);

        return result.Message;
    }

    /// <summary>
    /// Отримати статистику користувачів через збережувану процедуру
    /// </summary>
    public async Task<List<dynamic>> GetUsersStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        var stats = await connection.QueryAsync<dynamic>("GetUsersStatistics",
            commandType: CommandType.StoredProcedure);

        return stats.ToList();
    }

    /// <summary>
    /// Масове оновлення рівнів навичок через збережувану процедуру
    /// </summary>
    public async Task<int> BulkUpdateSkillLevelsAsync(int userId, string skillUpdates)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QuerySingleAsync<dynamic>("BulkUpdateSkillLevels",
            new { p_UserId = userId, p_SkillUpdates = skillUpdates },
            commandType: CommandType.StoredProcedure);

        return result.UpdatedCount;
    }

    // ===================================
    // КОМПЛЕКСНІ ЗАПИТИ З ЗВ'ЯЗКАМИ
    // ===================================

    /// <summary>
    /// Отримати користувачів з їх налаштуваннями та кількістю підписок
    /// </summary>
    public async Task<List<UserWithStatistics>> GetUsersWithStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                u.UserId,
                u.Email,
                u.FullName,
                u.Role,
                u.EnglishLevel,
                u.IsActive,
                COALESCE(s.Theme, 'Light') as Theme,
                COALESCE(s.Language, 'en') as Language,
                COALESCE(s.NotificationsEnabled, TRUE) as NotificationsEnabled,
                COUNT(DISTINCT sub.SubscriptionId) as ActiveSubscriptions,
                COUNT(DISTINCT usk.SkillId) as SkillsCount
            FROM UserProfiles u
            LEFT JOIN UserSettings s ON u.UserId = s.UserId
            LEFT JOIN UserSubscriptions sub ON u.UserId = sub.UserId AND sub.IsActive = TRUE
            LEFT JOIN UserSkillLevels usk ON u.UserId = usk.UserId
            GROUP BY u.UserId, u.Email, u.FullName, u.Role, u.EnglishLevel, u.IsActive,
                     s.Theme, s.Language, s.NotificationsEnabled
            ORDER BY u.CreatedAt DESC";

        var users = await connection.QueryAsync<UserWithStatistics>(sql);
        return users.ToList();
    }

    /// <summary>
    /// Отримати статистику навичок по категоріях
    /// </summary>
    public async Task<List<SkillCategoryStatistics>> GetSkillCategoryStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                s.Category,
                COUNT(DISTINCT s.SkillId) as TotalSkills,
                COUNT(DISTINCT usk.UserId) as UsersWithSkills,
                AVG(CASE 
                    WHEN usk.Level = 'Beginner' THEN 1
                    WHEN usk.Level = 'Intermediate' THEN 2
                    WHEN usk.Level = 'Advanced' THEN 3
                    WHEN usk.Level = 'Expert' THEN 4
                    ELSE 0
                END) as AverageLevel
            FROM Skills s
            LEFT JOIN UserSkillLevels usk ON s.SkillId = usk.SkillId
            GROUP BY s.Category
            ORDER BY s.Category";

        var stats = await connection.QueryAsync<SkillCategoryStatistics>(sql);
        return stats.ToList();
    }

    /// <summary>
    /// Пошук користувачів з фільтрами
    /// </summary>
    public async Task<List<UserProfile>> SearchUsersAsync(string? role = null, string? englishLevel = null, bool? isActive = null, string? searchTerm = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(role))
        {
            conditions.Add("Role = @Role");
            parameters.Add("Role", role);
        }

        if (!string.IsNullOrEmpty(englishLevel))
        {
            conditions.Add("EnglishLevel = @EnglishLevel");
            parameters.Add("EnglishLevel", englishLevel);
        }

        if (isActive.HasValue)
        {
            conditions.Add("IsActive = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            conditions.Add("(FullName LIKE @SearchTerm OR Email LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        string whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        string sql = $"SELECT * FROM UserProfiles {whereClause} ORDER BY CreatedAt DESC";

        var users = await connection.QueryAsync<UserProfile>(sql, parameters);
        return users.ToList();
    }
}

// ===================================
// Допоміжні класи (з UsersService)
// ===================================

public class UserStatistic
{
    public string Role { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
}