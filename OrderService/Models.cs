namespace OrderService;

/// <summary>
/// Модель замовлення
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
/// Модель продукту/категорії
/// </summary>
public class Product
{
    public int ProductId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Модель позиції замовлення (M:N між Orders та Products)
/// </summary>
public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Модель платежу (1:1 з Orders)
/// </summary>
public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Card, PayPal, Bank
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime PaymentDate { get; set; }
}

/// <summary>
/// Розширена модель замовлення з деталями
/// </summary>
public class OrderWithDetails
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
    public Payment? Payment { get; set; }
}

/// <summary>
/// Модель для створення замовлення
/// </summary>
public class CreateOrderRequest
{
    public int UserId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = new();
    public PaymentRequest Payment { get; set; } = new();
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
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

/// <summary>
/// Статистика продуктів
/// </summary>
public class ProductStatistic
{
    public int ProductId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? TotalSold { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? AveragePrice { get; set; }
}