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
    // CRUD для Products
    // ===================================

    /// <summary>
    /// Створити продукт
    /// </summary>
    public async Task<int> CreateProductAsync(string categoryName, decimal price, string description)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO Products (CategoryName, Price, Description, IsAvailable, CreatedAt)
            VALUES (@CategoryName, @Price, @Description, TRUE, NOW());
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new { CategoryName = categoryName, Price = price, Description = description });
    }

    /// <summary>
    /// Отримати всі продукти
    /// </summary>
    public async Task<List<Product>> GetAllProductsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Products ORDER BY CategoryName";

        var products = await connection.QueryAsync<Product>(sql);
        return products.ToList();
    }

    /// <summary>
    /// Отримати доступні продукти
    /// </summary>
    public async Task<List<Product>> GetAvailableProductsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Products WHERE IsAvailable = TRUE ORDER BY CategoryName";

        var products = await connection.QueryAsync<Product>(sql);
        return products.ToList();
    }

    /// <summary>
    /// Отримати продукт по ID
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Products WHERE ProductId = @ProductId";

        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductId = productId });
    }

    /// <summary>
    /// Оновити продукт
    /// </summary>
    public async Task<bool> UpdateProductAsync(int productId, string categoryName, decimal price, string description, bool isAvailable)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            UPDATE Products 
            SET CategoryName = @CategoryName, Price = @Price, Description = @Description, IsAvailable = @IsAvailable
            WHERE ProductId = @ProductId";

        int rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ProductId = productId,
            CategoryName = categoryName,
            Price = price,
            Description = description,
            IsAvailable = isAvailable
        });
        return rowsAffected > 0;
    }

    // ===================================
    // CRUD для Orders (розширений)
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
    /// Отримати замовлення з повними деталями
    /// </summary>
    public async Task<OrderWithDetails?> GetOrderWithDetailsAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        // Отримати основне замовлення
        string orderSql = "SELECT * FROM Orders WHERE OrderId = @OrderId";
        var order = await connection.QueryFirstOrDefaultAsync<Order>(orderSql, new { OrderId = orderId });

        if (order == null) return null;

        // Отримати позиції замовлення
        string itemsSql = @"
            SELECT oi.*, p.CategoryName as ProductCategoryName, p.Description as ProductDescription
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.ProductId
            WHERE oi.OrderId = @OrderId";
        var items = await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = orderId });

        // Отримати платіж
        string paymentSql = "SELECT * FROM Payments WHERE OrderId = @OrderId";
        var payment = await connection.QueryFirstOrDefaultAsync<Payment>(paymentSql, new { OrderId = orderId });

        return new OrderWithDetails
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            CategoryName = order.CategoryName,
            Price = order.Price,
            Status = order.Status,
            OrderDate = order.OrderDate,
            CompletedDate = order.CompletedDate,
            OrderItems = items.ToList(),
            Payment = payment
        };
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

    // ===================================
    // CRUD для OrderItems
    // ===================================

    /// <summary>
    /// Додати позицію до замовлення
    /// </summary>
    public async Task<int> AddOrderItemAsync(int orderId, int productId, int quantity)
    {
        using var connection = new MySqlConnection(_connectionString);

        // Отримати ціну продукту
        var product = await GetProductByIdAsync(productId);
        if (product == null || !product.IsAvailable)
            throw new ArgumentException("Product not found or not available");

        decimal unitPrice = product.Price;
        decimal lineTotal = unitPrice * quantity;

        string sql = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal);
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal
        });
    }

    /// <summary>
    /// Отримати позиції замовлення
    /// </summary>
    public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";

        var items = await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
        return items.ToList();
    }

    /// <summary>
    /// Видалити позицію замовлення
    /// </summary>
    public async Task<bool> DeleteOrderItemAsync(int orderItemId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "DELETE FROM OrderItems WHERE OrderItemId = @OrderItemId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { OrderItemId = orderItemId });
        return rowsAffected > 0;
    }

    // ===================================
    // CRUD для Payments
    // ===================================

    /// <summary>
    /// Створити платіж
    /// </summary>
    public async Task<int> CreatePaymentAsync(int orderId, decimal amount, string paymentMethod, string transactionId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            INSERT INTO Payments (OrderId, Amount, PaymentMethod, TransactionId, Status, PaymentDate)
            VALUES (@OrderId, @Amount, @PaymentMethod, @TransactionId, 'Pending', NOW());
            SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            TransactionId = transactionId
        });
    }

    /// <summary>
    /// Отримати платіж по замовленню
    /// </summary>
    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "SELECT * FROM Payments WHERE OrderId = @OrderId";

        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { OrderId = orderId });
    }

    /// <summary>
    /// Оновити статус платежу
    /// </summary>
    public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status, string? transactionId = null)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            UPDATE Payments 
            SET Status = @Status" +
            (transactionId != null ? ", TransactionId = @TransactionId" : "") +
            " WHERE PaymentId = @PaymentId";

        var parameters = new { PaymentId = paymentId, Status = status, TransactionId = transactionId };
        int rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    // ===================================
    // ЗБЕРЕЖУВАНІ ПРОЦЕДУРИ
    // ===================================

    /// <summary>
    /// Створити замовлення з позиціями через збережувану процедуру
    /// </summary>
    public async Task<(int orderId, decimal totalAmount)> CreateOrderWithItemsAsync(
        int userId,
        string categoryName,
        List<(int productId, int quantity)> items,
        string paymentMethod,
        string transactionId)
    {
        using var connection = new MySqlConnection(_connectionString);

        var productIds = string.Join(",", items.Select(i => i.productId));
        var quantities = string.Join(",", items.Select(i => i.quantity));

        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId);
        parameters.Add("p_CategoryName", categoryName);
        parameters.Add("p_ProductIds", productIds);
        parameters.Add("p_Quantities", quantities);
        parameters.Add("p_PaymentMethod", paymentMethod);
        parameters.Add("p_TransactionId", transactionId);
        parameters.Add("p_OrderId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        parameters.Add("p_TotalAmount", dbType: System.Data.DbType.Decimal, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("CreateOrderWithItems", parameters, commandType: System.Data.CommandType.StoredProcedure);

        return (parameters.Get<int>("p_OrderId"), parameters.Get<decimal>("p_TotalAmount"));
    }

    /// <summary>
    /// Підтвердити платіж через збережувану процедуру
    /// </summary>
    public async Task ConfirmPaymentAsync(int orderId, string transactionId)
    {
        using var connection = new MySqlConnection(_connectionString);

        await connection.ExecuteAsync("ConfirmPayment",
            new { p_OrderId = orderId, p_TransactionId = transactionId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    /// <summary>
    /// Скасувати замовлення через збережувану процедуру
    /// </summary>
    public async Task<string> CancelOrderAsync(int orderId, string reason)
    {
        using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QuerySingleAsync<dynamic>("CancelOrder",
            new { p_OrderId = orderId, p_Reason = reason },
            commandType: System.Data.CommandType.StoredProcedure);

        return result.Message;
    }

    /// <summary>
    /// Отримати деталі замовлення через збережувану процедуру
    /// </summary>
    public async Task<(Order? order, List<dynamic> items, Payment? payment)> GetOrderDetailsAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync("GetOrderDetails",
            new { p_OrderId = orderId },
            commandType: System.Data.CommandType.StoredProcedure);

        var order = multi.Read<Order>().FirstOrDefault();
        var items = multi.Read<dynamic>().ToList();
        var payment = multi.Read<Payment>().FirstOrDefault();

        return (order, items, payment);
    }

    /// <summary>
    /// Отримати статистику через збережувану процедуру
    /// </summary>
    public async Task<List<dynamic>> GetOrderStatisticsAsync(DateTime dateFrom, DateTime dateTo)
    {
        using var connection = new MySqlConnection(_connectionString);

        var stats = await connection.QueryAsync<dynamic>("GetOrderStatistics",
            new { p_DateFrom = dateFrom.Date, p_DateTo = dateTo.Date },
            commandType: System.Data.CommandType.StoredProcedure);

        return stats.ToList();
    }

    /// <summary>
    /// Отримати топ продуктів через збережувану процедуру
    /// </summary>
    public async Task<List<dynamic>> GetTopProductsAsync(int limit = 10)
    {
        using var connection = new MySqlConnection(_connectionString);

        var products = await connection.QueryAsync<dynamic>("GetTopProducts",
            new { p_Limit = limit },
            commandType: System.Data.CommandType.StoredProcedure);

        return products.ToList();
    }

    // ===================================
    // СТАТИСТИКА ТА АНАЛІТИКА
    // ===================================

    /// <summary>
    /// Отримати статистику продуктів
    /// </summary>
    public async Task<List<ProductStatistic>> GetProductStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                p.ProductId,
                p.CategoryName,
                SUM(oi.Quantity) as TotalSold,
                SUM(oi.LineTotal) as TotalRevenue,
                AVG(oi.UnitPrice) as AveragePrice
            FROM Products p
            LEFT JOIN OrderItems oi ON p.ProductId = oi.ProductId
            LEFT JOIN Orders o ON oi.OrderId = o.OrderId AND o.Status = 'Completed'
            GROUP BY p.ProductId, p.CategoryName
            ORDER BY TotalRevenue DESC";

        var stats = await connection.QueryAsync<ProductStatistic>(sql);
        return stats.ToList();
    }

    /// <summary>
    /// Отримати загальну статистику системи
    /// </summary>
    public async Task<dynamic> GetGeneralStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = @"
            SELECT 
                COUNT(DISTINCT o.OrderId) as TotalOrders,
                COUNT(DISTINCT CASE WHEN o.Status = 'Completed' THEN o.OrderId END) as CompletedOrders,
                COUNT(DISTINCT CASE WHEN o.Status = 'Pending' THEN o.OrderId END) as PendingOrders,
                COUNT(DISTINCT CASE WHEN o.Status = 'Cancelled' THEN o.OrderId END) as CancelledOrders,
                SUM(CASE WHEN o.Status = 'Completed' THEN o.Price ELSE 0 END) as TotalRevenue,
                AVG(CASE WHEN o.Status = 'Completed' THEN o.Price END) as AverageOrderValue,
                COUNT(DISTINCT o.UserId) as UniqueCustomers,
                COUNT(DISTINCT p.ProductId) as TotalProducts,
                SUM(CASE WHEN p.IsAvailable = TRUE THEN 1 ELSE 0 END) as AvailableProducts
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.ProductId";

        return await connection.QueryFirstAsync(sql);
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

    /// <summary>
    /// Видалити замовлення (каскадно видалить позиції та платежі)
    /// </summary>
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        using var connection = new MySqlConnection(_connectionString);

        string sql = "DELETE FROM Orders WHERE OrderId = @OrderId";

        int rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId });
        return rowsAffected > 0;
    }
}