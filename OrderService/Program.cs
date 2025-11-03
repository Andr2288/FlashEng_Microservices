using OrderService;

namespace FlashEngOrders
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Order Service (РОЗШИРЕНИЙ)");
            Console.WriteLine("  4 Таблиці + Зв'язки + 6 Процедур");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних та таблиці
                await DatabaseConfig.EnsureDatabaseCreatedAsync();
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new OrderRepository();

                Console.WriteLine("🎯 ДЕМОНСТРАЦІЯ ЗВ'ЯЗКІВ МІЖ ТАБЛИЦЯМИ");
                Console.WriteLine("=====================================");

                // 1. Показати Products (база для зв'язків)
                Console.WriteLine("\n--- 📦 ALL PRODUCTS ---");
                var products = await repository.GetAllProductsAsync();
                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.ProductId} | {product.CategoryName} | ${product.Price} | {(product.IsAvailable ? "✅" : "❌")}");
                }

                // 2. Створити замовлення ЧЕРЕЗ ЗБЕРЕЖУВАНУ ПРОЦЕДУРУ
                Console.WriteLine("\n--- 🛒 СТВОРЕННЯ ЗАМОВЛЕННЯ ЧЕРЕЗ ПРОЦЕДУРУ ---");
                try
                {
                    var orderItems = new List<(int productId, int quantity)>
                    {
                        (1, 2), // Business English x2
                        (3, 1)  // Advanced Grammar x1
                    };

                    var (newOrderId, totalAmount) = await repository.CreateOrderWithItemsAsync(
                        userId: 1,
                        categoryName: "Mixed Bundle",
                        items: orderItems,
                        paymentMethod: "Card",
                        transactionId: $"TXN_{DateTime.Now:yyyyMMddHHmmss}"
                    );

                    Console.WriteLine($"✅ Замовлення створено: OrderId = {newOrderId}, Сума = ${totalAmount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка створення замовлення: {ex.Message}");
                }

                // 3. Показати зв'язки 1:N (Orders → OrderItems)
                Console.WriteLine("\n--- 🔗 ЗВ'ЯЗОК 1:N (Orders → OrderItems) ---");
                var allOrders = await repository.GetAllOrdersAsync();
                foreach (var order in allOrders.Take(3))
                {
                    Console.WriteLine($"\n📋 Order #{order.OrderId} (User {order.UserId}) - {order.Status}");
                    var items = await repository.GetOrderItemsAsync(order.OrderId);
                    foreach (var item in items)
                    {
                        var product = await repository.GetProductByIdAsync(item.ProductId);
                        Console.WriteLine($"   └── {product?.CategoryName ?? "Unknown"} x{item.Quantity} = ${item.LineTotal}");
                    }
                }

                // 4. Показати зв'язки 1:1 (Orders ↔ Payments)
                Console.WriteLine("\n--- 🔗 ЗВ'ЯЗОК 1:1 (Orders ↔ Payments) ---");
                foreach (var order in allOrders.Take(3))
                {
                    var payment = await repository.GetPaymentByOrderIdAsync(order.OrderId);
                    if (payment != null)
                    {
                        Console.WriteLine($"💳 Order #{order.OrderId} → Payment #{payment.PaymentId} | {payment.PaymentMethod} | ${payment.Amount} | {payment.Status}");
                    }
                }

                // 5. Показати зв'язки M:N (Orders ↔ Products через OrderItems)
                Console.WriteLine("\n--- 🔗 ЗВ'ЯЗОК M:N (Orders ↔ Products) ---");
                foreach (var product in products.Take(3))
                {
                    Console.WriteLine($"\n📦 Product: {product.CategoryName}");
                    var ordersWithProduct = await repository.SearchOrdersAsync();
                    var relatedOrders = new List<Order>();

                    foreach (var order in ordersWithProduct)
                    {
                        var items = await repository.GetOrderItemsAsync(order.OrderId);
                        if (items.Any(item => item.ProductId == product.ProductId))
                        {
                            relatedOrders.Add(order);
                        }
                    }

                    foreach (var order in relatedOrders.Take(2))
                    {
                        Console.WriteLine($"   └── використовується в Order #{order.OrderId} ({order.Status})");
                    }
                }

                Console.WriteLine("\n🎯 ДЕМОНСТРАЦІЯ ЗБЕРЕЖУВАНИХ ПРОЦЕДУР");
                Console.WriteLine("=====================================");

                // 6. Процедура: Отримати деталі замовлення
                Console.WriteLine("\n--- 📊 ПРОЦЕДУРА: GetOrderDetails ---");
                try
                {
                    var (orderDetails, itemDetails, paymentDetails) = await repository.GetOrderDetailsAsync(1);
                    if (orderDetails != null)
                    {
                        Console.WriteLine($"📋 Order: #{orderDetails.OrderId} | {orderDetails.Status} | ${orderDetails.Price}");
                        Console.WriteLine("   Позиції:");
                        foreach (var item in itemDetails)
                        {
                            Console.WriteLine($"   └── {item.CategoryName} x{item.Quantity} = ${item.LineTotal}");
                        }
                        if (paymentDetails != null)
                        {
                            Console.WriteLine($"   💳 Payment: {paymentDetails.PaymentMethod} | {paymentDetails.Status}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка процедури GetOrderDetails: {ex.Message}");
                }

                // 7. Процедура: Підтвердити платіж
                Console.WriteLine("\n--- 💳 ПРОЦЕДУРА: ConfirmPayment ---");
                try
                {
                    await repository.ConfirmPaymentAsync(1, "CONFIRMED_TXN_001");
                    Console.WriteLine("✅ Платіж підтверджено для Order #1");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка підтвердження платежу: {ex.Message}");
                }

                // 8. Процедура: Статистика за період
                Console.WriteLine("\n--- 📈 ПРОЦЕДУРА: GetOrderStatistics ---");
                try
                {
                    var dateFrom = new DateTime(2024, 10, 1);
                    var dateTo = DateTime.Now;
                    var stats = await repository.GetOrderStatisticsAsync(dateFrom, dateTo);

                    Console.WriteLine($"Статистика з {dateFrom:yyyy-MM-dd} по {dateTo:yyyy-MM-dd}:");
                    foreach (var stat in stats)
                    {
                        Console.WriteLine($"📊 {stat.CategoryName}: {stat.OrderCount} замовлень | ${stat.TotalRevenue} доходу | середній чек ${stat.AverageOrderValue:F2}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка статистики: {ex.Message}");
                }

                // 9. Процедура: Топ продуктів
                Console.WriteLine("\n--- 🏆 ПРОЦЕДУРА: GetTopProducts ---");
                try
                {
                    var topProducts = await repository.GetTopProductsAsync(5);
                    Console.WriteLine("Топ-5 продуктів:");
                    foreach (var product in topProducts)
                    {
                        Console.WriteLine($"🥇 {product.CategoryName}: {product.TotalSold} продано | ${product.TotalRevenue} доходу | {product.OrderCount} замовлень");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка топ продуктів: {ex.Message}");
                }

                // 10. Процедура: Скасувати замовлення
                Console.WriteLine("\n--- ❌ ПРОЦЕДУРА: CancelOrder ---");
                try
                {
                    // Знайти замовлення зі статусом Pending
                    var pendingOrders = await repository.SearchOrdersAsync(status: "Pending");
                    if (pendingOrders.Any())
                    {
                        var orderToCancel = pendingOrders.First();
                        var message = await repository.CancelOrderAsync(orderToCancel.OrderId, "Customer request");
                        Console.WriteLine($"📝 {message}");
                    }
                    else
                    {
                        Console.WriteLine("Немає замовлень для скасування");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка скасування: {ex.Message}");
                }

                Console.WriteLine("\n🎯 ДОДАТКОВІ CRUD ОПЕРАЦІЇ");
                Console.WriteLine("==========================");

                // 11. Створити новий продукт
                Console.WriteLine("\n--- ➕ СТВОРЕННЯ НОВОГО ПРОДУКТУ ---");
                try
                {
                    int newProductId = await repository.CreateProductAsync(
                        categoryName: "Pronunciation Practice",
                        price: 34.99m,
                        description: "Advanced pronunciation training with AI feedback"
                    );
                    Console.WriteLine($"✅ Новий продукт створено: ID = {newProductId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка створення продукту: {ex.Message}");
                }

                // 12. Додати позицію до існуючого замовлення
                Console.WriteLine("\n--- ➕ ДОДАВАННЯ ПОЗИЦІЇ ДО ЗАМОВЛЕННЯ ---");
                try
                {
                    var pendingOrders = await repository.SearchOrdersAsync(status: "Pending");
                    if (pendingOrders.Any())
                    {
                        var orderId = pendingOrders.First().OrderId;
                        int newItemId = await repository.AddOrderItemAsync(orderId, 2, 1); // Travel Phrases x1
                        Console.WriteLine($"✅ Позицію додано: OrderItemId = {newItemId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка додавання позиції: {ex.Message}");
                }

                // 13. Замовлення з повними деталями
                Console.WriteLine("\n--- 📋 ЗАМОВЛЕННЯ З ПОВНИМИ ДЕТАЛЯМИ ---");
                var orderWithDetails = await repository.GetOrderWithDetailsAsync(1);
                if (orderWithDetails != null)
                {
                    Console.WriteLine($"📋 Order #{orderWithDetails.OrderId}:");
                    Console.WriteLine($"   User: {orderWithDetails.UserId}");
                    Console.WriteLine($"   Status: {orderWithDetails.Status}");
                    Console.WriteLine($"   Total: ${orderWithDetails.Price}");
                    Console.WriteLine($"   Items: {orderWithDetails.OrderItems.Count}");
                    Console.WriteLine($"   Payment: {orderWithDetails.Payment?.PaymentMethod ?? "None"}");
                }

                // 14. Загальна статистика системи
                Console.WriteLine("\n--- 📊 ЗАГАЛЬНА СТАТИСТИКА СИСТЕМИ ---");
                var generalStats = await repository.GetGeneralStatisticsAsync();
                Console.WriteLine($"📈 Загальні показники:");
                Console.WriteLine($"   Всього замовлень: {generalStats.TotalOrders}");
                Console.WriteLine($"   Завершено: {generalStats.CompletedOrders}");
                Console.WriteLine($"   В очікуванні: {generalStats.PendingOrders}");
                Console.WriteLine($"   Скасовано: {generalStats.CancelledOrders}");
                Console.WriteLine($"   Загальний дохід: ${generalStats.TotalRevenue}");
                Console.WriteLine($"   Середній чек: ${generalStats.AverageOrderValue:F2}");
                Console.WriteLine($"   Унікальних клієнтів: {generalStats.UniqueCustomers}");
                Console.WriteLine($"   Всього продуктів: {generalStats.TotalProducts}");
                Console.WriteLine($"   Доступних продуктів: {generalStats.AvailableProducts}");

                // 15. Статистика продуктів
                Console.WriteLine("\n--- 📊 СТАТИСТИКА ПРОДУКТІВ ---");
                var productStats = await repository.GetProductStatisticsAsync();
                foreach (var stat in productStats.Where(s => s.TotalSold.HasValue && s.TotalSold.Value > 0))
                {
                    Console.WriteLine($"📦 {stat.CategoryName}: {stat.TotalSold?.ToString() ?? "0"} продано | ${stat.TotalRevenue?.ToString("F2") ?? "0.00"} доходу");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  ✅ УСПІШНО ПРОДЕМОНСТРОВАНО:");
                Console.WriteLine("  📊 4 таблиці з Foreign Key зв'язками");
                Console.WriteLine("  🔗 Зв'язки: 1:1, 1:N, M:N");
                Console.WriteLine("  ⚙️ 6 збережуваних процедур");
                Console.WriteLine("  🛠️ Повний CRUD для всіх сутностей");
                Console.WriteLine("  📈 Транзакційна бізнес-логіка");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ КРИТИЧНА ПОМИЛКА: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}