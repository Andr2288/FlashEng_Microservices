using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace OrderService;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    // ===================================
    // ОСНОВНІ CRUD ОПЕРАЦІЇ
    // ===================================

    /// <summary>
    /// Отримати всі продукти
    /// </summary>
    public async Task<List<Product>> GetAllProductsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM Products ORDER BY Name";
        var products = await connection.QueryAsync<Product>(sql);
        return products.ToList();
    }

    /// <summary>
    /// Отримати всі замовлення
    /// </summary>
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";
        var orders = await connection.QueryAsync<Order>(sql);
        return orders.ToList();
    }

    /// <summary>
    /// Створити продукт
    /// </summary>
    public async Task<int> CreateProductAsync(string name, decimal price)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = @"
            INSERT INTO Products (Name, Price, IsAvailable)
            VALUES (@Name, @Price, TRUE);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new { Name = name, Price = price });
    }

    /// <summary>
    /// Створити замовлення (простий спосіб)
    /// </summary>
    public async Task<int> CreateOrderAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = @"
            INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate)
            VALUES (@UserId, 0, 'Pending', NOW());
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Додати товар до замовлення
    /// </summary>
    public async Task AddOrderItemAsync(int orderId, int productId, int quantity)
    {
        using var connection = new MySqlConnection(_connectionString);

        // Отримати ціну продукту
        var product = await connection.QueryFirstAsync<Product>(
            "SELECT * FROM Products WHERE ProductId = @ProductId",
            new { ProductId = productId });

        decimal lineTotal = product.Price * quantity;

        // Додати позицію
        await connection.ExecuteAsync(@"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal)",
            new
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            });

        // Оновити загальну суму замовлення
        await connection.ExecuteAsync(@"
            UPDATE Orders 
            SET TotalAmount = (SELECT SUM(LineTotal) FROM OrderItems WHERE OrderId = @OrderId)
            WHERE OrderId = @OrderId",
            new { OrderId = orderId });
    }

    // ===================================
    // ЗБЕРЕЖУВАНІ ПРОЦЕДУРИ
    // ===================================

    /// <summary>
    /// Створити замовлення з товарами через збережувану процедуру
    /// </summary>
    public async Task<int> CreateOrderWithItemsAsync(int userId, List<(int productId, int quantity)> items)
    {
        using var connection = new MySqlConnection(_connectionString);

        var productIds = string.Join(",", items.Select(i => i.productId));
        var quantities = string.Join(",", items.Select(i => i.quantity));

        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId);
        parameters.Add("p_ProductIds", productIds);
        parameters.Add("p_Quantities", quantities);
        parameters.Add("p_OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("CreateOrderWithItems", parameters, commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("p_OrderId");
    }

    /// <summary>
    /// Отримати деталі замовлення через збережувану процедуру
    /// </summary>
    public async Task<OrderDetails> GetOrderDetailsAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync("GetOrderDetails",
            new { p_OrderId = orderId },
            commandType: CommandType.StoredProcedure);

        var order = multi.Read<Order>().FirstOrDefault();
        var items = multi.Read<OrderItemDetails>().ToList();
        var payment = multi.Read<Payment>().FirstOrDefault();

        if (order == null)
            throw new ArgumentException("Order not found");

        return new OrderDetails
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            OrderDate = order.OrderDate,
            Items = items,
            Payment = payment
        };
    }

    /// <summary>
    /// Змінити статус замовлення через збережувану процедуру
    /// </summary>
    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
        using var connection = new MySqlConnection(_connectionString);

        await connection.ExecuteAsync("UpdateOrderStatus",
            new { p_OrderId = orderId, p_Status = status },
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Отримати статистику через збережувану процедуру
    /// </summary>
    public async Task<dynamic> GetOrderStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        var stats = await connection.QuerySingleAsync<dynamic>("GetOrderStatistics",
            commandType: CommandType.StoredProcedure);

        return stats;
    }

    // ===================================
    // ДОДАТКОВІ МЕТОДИ
    // ===================================

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
    /// Пошук продуктів
    /// </summary>
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        using var connection = new MySqlConnection(_connectionString);
        string sql = "SELECT * FROM Products WHERE Name LIKE @SearchTerm AND IsAvailable = TRUE";
        var products = await connection.QueryAsync<Product>(sql, new { SearchTerm = $"%{searchTerm}%" });
        return products.ToList();
    }

    /// <summary>
    /// Створити платіж для замовлення
    /// </summary>
    public async Task CreatePaymentAsync(int orderId, string paymentMethod)
    {
        using var connection = new MySqlConnection(_connectionString);

        // Отримати суму замовлення
        var order = await connection.QueryFirstAsync<Order>(
            "SELECT * FROM Orders WHERE OrderId = @OrderId",
            new { OrderId = orderId });

        await connection.ExecuteAsync(@"
            INSERT INTO Payments (OrderId, Amount, PaymentMethod, Status, PaymentDate)
            VALUES (@OrderId, @Amount, @PaymentMethod, 'Completed', NOW())",
            new
            {
                OrderId = orderId,
                Amount = order.TotalAmount,
                PaymentMethod = paymentMethod
            });
    }
}