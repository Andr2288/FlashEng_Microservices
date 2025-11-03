namespace OrderService;

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
/// Модель для показу деталей замовлення
/// </summary>
public class OrderDetails
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
}

/// <summary>
/// Статистика замовлень
/// </summary>
public class OrderStatistic
{
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
}

/// <summary>
/// Статистика по статусах
/// </summary>
public class StatusStatistic
{
    public string Status { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}