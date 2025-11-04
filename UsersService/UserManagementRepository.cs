using MySql.Data.MySqlClient;
using Dapper;
using System.Data;

namespace UserManagementService;

/// <summary>
/// Спрощений репозиторій для управління користувачами
/// </summary>
public class UserManagementRepository
{
    private readonly string _connectionString;

    public UserManagementRepository()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    // ===================================
    // CRUD для UserProfiles
    // ===================================

    public async Task<List<UserProfile>> GetAllUsersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM UserProfiles ORDER BY CreatedAt DESC";
        var users = await connection.QueryAsync<UserProfile>(sql);
        return users.ToList();
    }

    public async Task<UserProfile?> GetUserByIdAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM UserProfiles WHERE UserId = @UserId";
        return await connection.QueryFirstOrDefaultAsync<UserProfile>(sql, new { UserId = userId });
    }

    public async Task<int> CreateUserAsync(string email, string password, string fullName)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = @"
            INSERT INTO UserProfiles (Email, PasswordHash, FullName, IsActive)
            VALUES (@Email, @Password, @FullName, TRUE);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            Email = email,
            Password = password,
            FullName = fullName
        });
    }

    public async Task<int> UpdateUserAsync(int userId, string? fullName = null, string? role = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        var setParts = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (fullName != null)
        {
            setParts.Add("FullName = @FullName");
            parameters.Add("FullName", fullName);
        }

        if (role != null)
        {
            setParts.Add("Role = @Role");
            parameters.Add("Role", role);
        }

        if (!setParts.Any()) return 0;

        string sql = $"UPDATE UserProfiles SET {string.Join(", ", setParts)} WHERE UserId = @UserId";
        return await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<int> DeleteUserAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "DELETE FROM UserProfiles WHERE UserId = @UserId";
        return await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    // ===================================
    // CRUD для UserSettings (1:1 зв'язок)
    // ===================================

    public async Task<UserSettings?> GetUserSettingsAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM UserSettings WHERE UserId = @UserId";
        return await connection.QueryFirstOrDefaultAsync<UserSettings>(sql, new { UserId = userId });
    }

    public async Task<int> CreateUserSettingsAsync(int userId, string theme = "Light", string language = "en")
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = @"
            INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled)
            VALUES (@UserId, @Theme, @Language, TRUE);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            UserId = userId,
            Theme = theme,
            Language = language
        });
    }

    public async Task<int> UpdateUserSettingsAsync(int userId, string? theme = null, string? language = null, bool? notifications = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        var setParts = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (theme != null)
        {
            setParts.Add("Theme = @Theme");
            parameters.Add("Theme", theme);
        }

        if (language != null)
        {
            setParts.Add("Language = @Language");
            parameters.Add("Language", language);
        }

        if (notifications.HasValue)
        {
            setParts.Add("NotificationsEnabled = @Notifications");
            parameters.Add("Notifications", notifications.Value);
        }

        if (!setParts.Any()) return 0;

        string sql = $"UPDATE UserSettings SET {string.Join(", ", setParts)} WHERE UserId = @UserId";
        return await connection.ExecuteAsync(sql, parameters);
    }

    // ===================================
    // ЗБЕРЕЖУВАНА ПРОЦЕДУРА
    // ===================================

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
}