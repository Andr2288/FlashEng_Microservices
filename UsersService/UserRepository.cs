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

    // ===================================
    // CRUD для UserProfile (спрощено)
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
    /// Створити нового користувача через збережувану процедуру
    /// </summary>
    public async Task<int> CreateUserAsync(string email, string password, string fullName, string englishLevel = "A1", string role = "User")
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<NewUserResult>(
            "CreateUser",
            new
            {
                p_Email = email,
                p_Password = password,
                p_FullName = fullName,
                p_EnglishLevel = englishLevel,
                p_Role = role
            },
            commandType: CommandType.StoredProcedure
        );

        return result?.NewUserId ?? 0;
    }

    /// <summary>
    /// Створити користувача з транзакцією (спрощено)
    /// </summary>
    public async Task<int> CreateUserWithTransactionAsync(string email, string password, string fullName, string englishLevel = "A1")
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string insertUserSql = @"
                INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, EnglishLevel, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Email, @Password, @FullName, 'User', @EnglishLevel, 1, NOW(), NOW());
                SELECT LAST_INSERT_ID();";

            int newUserId = await connection.QuerySingleAsync<int>(
                insertUserSql,
                new { Email = email, Password = password, FullName = fullName, EnglishLevel = englishLevel },
                transaction
            );

            await transaction.CommitAsync();
            return newUserId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Оновити профіль користувача
    /// </summary>
    public async Task<int> UpdateUserProfileAsync(int userId, string? englishLevel = null, int? dailyGoal = null, bool? notificationsEnabled = null)
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

    /// <summary>
    /// Деактивувати користувача
    /// </summary>
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "UPDATE UserProfiles SET IsActive = 0, UpdatedAt = NOW() WHERE UserId = @UserId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
        return rowsAffected > 0;
    }

    // ===================================
    // CRUD для Orders (нова функціональність)
    // ===================================

    /// <summary>
    /// Створити замовлення
    /// </summary>
    public async Task<int> CreateOrderAsync(int userId, string categoryName, decimal price)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO Orders (UserId, CategoryName, Price, Status, OrderDate)
            VALUES (@UserId, @CategoryName, @Price, 'Pending', NOW());
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new { UserId = userId, CategoryName = categoryName, Price = price });
    }

    /// <summary>
    /// Отримати замовлення користувача
    /// </summary>
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, new { UserId = userId });
        return orders.ToList();
    }

    /// <summary>
    /// Оновити статус замовлення
    /// </summary>
    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            UPDATE Orders 
            SET Status = @Status, CompletedDate = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedDate END
            WHERE OrderId = @OrderId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId, Status = status });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Отримати всі замовлення (для адміністратора)
    /// </summary>
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql);
        return orders.ToList();
    }

    // ===================================
    // Статистика користувачів
    // ===================================

    /// <summary>
    /// Отримати статистику користувачів по ролях
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
}

// ===================================
// Допоміжні класи
// ===================================

public class NewUserResult
{
    public int NewUserId { get; set; }
}

public class RowsAffectedResult
{
    public int RowsAffected { get; set; }
}

public class UserStatistic
{
    public string Role { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
}