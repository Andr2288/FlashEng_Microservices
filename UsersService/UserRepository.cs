using MySql.Data.MySqlClient;
using Dapper;
using System.Data;

namespace UsersService;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Users ORDER BY CreatedAt DESC";

        var users = await connection.QueryAsync<User>(sql);
        return users.ToList();
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Users WHERE UserId = @UserId";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<UserInfo> GetUserInfoAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<UserInfo>(
            "GetUserInfo",
            new { p_UserId = userId },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> CreateUserAsync(string email, string password, string fullName, string englishLevel)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<NewUserResult>(
            "CreateUser",
            new
            {
                p_Email = email,
                p_Password = password,
                p_FullName = fullName,
                p_EnglishLevel = englishLevel
            },
            commandType: CommandType.StoredProcedure
        );

        return result?.NewUserId ?? 0;
    }

    public async Task<int> CreateUserWithTransactionAsync(string email, string password, string fullName, string englishLevel)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Починаємо транзакцію
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Крок 1: Додаємо користувача
            string insertUserSql = @"
                INSERT INTO Users (Email, PasswordHash, FullName, IsActive)
                VALUES (@Email, @Password, @FullName, 1);
                SELECT LAST_INSERT_ID();";

            int newUserId = await connection.QuerySingleAsync<int>(
                insertUserSql,
                new { Email = email, Password = password, FullName = fullName },
                transaction
            );

            // Крок 2: Додаємо профіль
            string insertProfileSql = @"
                INSERT INTO UserProfiles (UserId, EnglishLevel, PreferredAIModel, DailyGoal, NotificationsEnabled)
                VALUES (@UserId, @EnglishLevel, 'GPT-3.5', 10, 1);";

            await connection.ExecuteAsync(
                insertProfileSql,
                new { UserId = newUserId, EnglishLevel = englishLevel },
                transaction
            );

            // Крок 3: Додаємо роль User (RoleId = 2)
            string insertRoleSql = @"
                INSERT INTO UserRoles (UserId, RoleId)
                VALUES (@UserId, 2);";

            await connection.ExecuteAsync(
                insertRoleSql,
                new { UserId = newUserId },
                transaction
            );

            await transaction.CommitAsync();

            Console.WriteLine("Transaction committed successfully!");
            return newUserId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
            throw;
        }
    }

    public async Task<int> UpdateUserProfileAsync(int userId, string englishLevel, int dailyGoal, bool notificationsEnabled)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<RowsAffectedResult>(
            "UpdateUserProfile",
            new
            {
                p_UserId = userId,
                p_EnglishLevel = englishLevel,
                p_DailyGoal = dailyGoal,
                p_NotificationsEnabled = notificationsEnabled
            },
            commandType: CommandType.StoredProcedure
        );

        return result?.RowsAffected ?? 0;
    }

    public async Task<int> DeactivateUserAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<RowsAffectedResult>(
            "DeactivateUser",
            new { p_UserId = userId },
            commandType: CommandType.StoredProcedure
        );

        return result?.RowsAffected ?? 0;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Roles";

        var roles = await connection.QueryAsync<Role>(sql);
        return roles.ToList();
    }
}

public class NewUserResult
{
    public int NewUserId { get; set; }
}

public class RowsAffectedResult
{
    public int RowsAffected { get; set; }
}