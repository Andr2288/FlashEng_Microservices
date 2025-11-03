using MySql.Data.MySqlClient;
using Dapper;

namespace OrderService;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    // ===================================
    // CRUD для Orders
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
    /// Отримати замовлення по ID
    /// </summary>
    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders WHERE OrderId = @OrderId";

        return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderId = orderId });
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
    /// Отримати всі замовлення (для адміністратора)
    /// </summary>
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql);
        return orders.ToList();
    }

    /// <summary>
    /// Отримати замовлення по статусу
    /// </summary>
    public async Task<List<Order>> GetOrdersByStatusAsync(string status)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders WHERE Status = @Status ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, new { Status = status });
        return orders.ToList();
    }

    /// <summary>
    /// Отримати замовлення по категорії
    /// </summary>
    public async Task<List<Order>> GetOrdersByCategoryAsync(string categoryName)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Orders WHERE CategoryName = @CategoryName ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, new { CategoryName = categoryName });
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
            SET Status = @Status, 
                CompletedDate = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedDate END
            WHERE OrderId = @OrderId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId, Status = status });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Видалити замовлення
    /// </summary>
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "DELETE FROM Orders WHERE OrderId = @OrderId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId });
        return rowsAffected > 0;
    }

    // ===================================
    // Пошук та фільтрація
    // ===================================

    /// <summary>
    /// Пошук замовлень за період
    /// </summary>
    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT * FROM Orders 
            WHERE OrderDate >= @StartDate AND OrderDate <= @EndDate 
            ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, new { StartDate = startDate, EndDate = endDate });
        return orders.ToList();
    }

    /// <summary>
    /// Пошук замовлень з фільтрами
    /// </summary>
    public async Task<List<Order>> SearchOrdersAsync(int? userId = null, string? status = null, string? categoryName = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (userId.HasValue)
        {
            conditions.Add("UserId = @UserId");
            parameters.Add("UserId", userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            conditions.Add("Status = @Status");
            parameters.Add("Status", status);
        }

        if (!string.IsNullOrEmpty(categoryName))
        {
            conditions.Add("CategoryName = @CategoryName");
            parameters.Add("CategoryName", categoryName);
        }

        string whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        string sql = $"SELECT * FROM Orders {whereClause} ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, parameters);
        return orders.ToList();
    }

    // ===================================
    // Статистика та аналітика
    // ===================================

    /// <summary>
    /// Отримати статистику замовлень по категоріях
    /// </summary>
    public async Task<List<OrderStatistic>> GetCategoryStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                CategoryName,
                COUNT(*) as OrderCount,
                SUM(Price) as TotalRevenue,
                AVG(Price) as AveragePrice
            FROM Orders 
            WHERE Status = 'Completed'
            GROUP BY CategoryName
            ORDER BY TotalRevenue DESC";

        var stats = await connection.QueryAsync<OrderStatistic>(sql);
        return stats.ToList();
    }

    /// <summary>
    /// Отримати статистику по статусах
    /// </summary>
    public async Task<List<StatusStatistic>> GetStatusStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                Status,
                COUNT(*) as OrderCount,
                SUM(Price) as TotalAmount
            FROM Orders 
            GROUP BY Status
            ORDER BY OrderCount DESC";

        var stats = await connection.QueryAsync<StatusStatistic>(sql);
        return stats.ToList();
    }

    /// <summary>
    /// Отримати загальну статистику
    /// </summary>
    public async Task<dynamic> GetGeneralStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                COUNT(*) as TotalOrders,
                SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as CompletedOrders,
                SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingOrders,
                SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) as CancelledOrders,
                SUM(CASE WHEN Status = 'Completed' THEN Price ELSE 0 END) as TotalRevenue,
                AVG(CASE WHEN Status = 'Completed' THEN Price ELSE NULL END) as AverageOrderValue,
                COUNT(DISTINCT UserId) as UniqueCustomers,
                COUNT(DISTINCT CategoryName) as UniqueCategories
            FROM Orders";

        return await connection.QueryFirstAsync(sql);
    }

    /// <summary>
    /// Отримати топ користувачів по сумі замовлень
    /// </summary>
    public async Task<List<dynamic>> GetTopCustomersAsync(int topCount = 10)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                UserId,
                COUNT(*) as OrderCount,
                SUM(CASE WHEN Status = 'Completed' THEN Price ELSE 0 END) as TotalSpent,
                AVG(CASE WHEN Status = 'Completed' THEN Price ELSE NULL END) as AverageOrderValue
            FROM Orders 
            GROUP BY UserId
            ORDER BY TotalSpent DESC
            LIMIT @TopCount";

        var customers = await connection.QueryAsync(sql, new { TopCount = topCount });
        return customers.ToList();
    }

    /// <summary>
    /// Отримати замовлення за останні N днів
    /// </summary>
    public async Task<List<Order>> GetRecentOrdersAsync(int days = 7)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT * FROM Orders 
            WHERE OrderDate >= DATE_SUB(NOW(), INTERVAL @Days DAY)
            ORDER BY OrderDate DESC";

        var orders = await connection.QueryAsync<Order>(sql, new { Days = days });
        return orders.ToList();
    }
}