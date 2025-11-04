namespace OrderService;

/// <summary>
/// Продукт/Товар
/// </summary>
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Замовлення
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    public DateTime OrderDate { get; set; }
}

/// <summary>
/// Позиція замовлення (M:N між Orders та Products)
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
/// Платіж (1:1 з Orders)
/// </summary>
public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Card, PayPal, Bank
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime PaymentDate { get; set; }
}

/// <summary>
/// Детальна інформація про замовлення (для JOIN запитів)
/// </summary>
public class OrderDetails
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public List<OrderItemDetails> Items { get; set; } = new();
    public Payment? Payment { get; set; }
}

/// <summary>
/// Детальна інформація про позицію замовлення
/// </summary>
public class OrderItemDetails
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}