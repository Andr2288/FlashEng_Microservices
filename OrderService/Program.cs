using OrderService;

namespace FlashEngOrders
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Order Service");
            Console.WriteLine("  Orders + ADO.NET + Dapper");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних, якщо її не існує
                await DatabaseConfig.EnsureDatabaseCreatedAsync();

                // Створюємо таблиці, якщо їх не існує
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new OrderRepository();

                // 1. Показати всі замовлення
                Console.WriteLine("--- All Orders ---");
                var allOrders = await repository.GetAllOrdersAsync();
                foreach (var order in allOrders.Take(5)) // показати перші 5
                {
                    Console.WriteLine($"Order #{order.OrderId}: User {order.UserId} | {order.CategoryName} | ${order.Price} | {order.Status}");
                }

                // 2. Створити нові замовлення
                Console.WriteLine("\n--- Creating New Orders ---");
                try
                {
                    int orderId1 = await repository.CreateOrderAsync(1, "Advanced Grammar", 39.99m);
                    int orderId2 = await repository.CreateOrderAsync(2, "IELTS Preparation", 49.99m);

                    Console.WriteLine($"Order 1 created with ID: {orderId1}");
                    Console.WriteLine($"Order 2 created with ID: {orderId2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Order creation failed: {ex.Message}");
                }

                // 3. Показати замовлення користувача
                Console.WriteLine("\n--- User Orders (User ID = 1) ---");
                var userOrders = await repository.GetUserOrdersAsync(1);
                foreach (var order in userOrders)
                {
                    Console.WriteLine($"Order #{order.OrderId}: {order.CategoryName} - ${order.Price} - {order.Status}");
                }

                // 4. Оновити статус замовлення
                if (userOrders.Any(o => o.Status == "Pending"))
                {
                    Console.WriteLine("\n--- Updating Order Status ---");
                    var pendingOrder = userOrders.First(o => o.Status == "Pending");
                    bool updated = await repository.UpdateOrderStatusAsync(pendingOrder.OrderId, "Completed");
                    Console.WriteLine($"Order #{pendingOrder.OrderId} status updated: {updated}");
                }

                // 5. Замовлення по статусу
                Console.WriteLine("\n--- Completed Orders ---");
                var completedOrders = await repository.GetOrdersByStatusAsync("Completed");
                foreach (var order in completedOrders.Take(3))
                {
                    Console.WriteLine($"Order #{order.OrderId}: {order.CategoryName} - ${order.Price} (Completed: {order.CompletedDate})");
                }

                // 6. Замовлення по категорії
                Console.WriteLine("\n--- Business English Orders ---");
                var businessOrders = await repository.GetOrdersByCategoryAsync("Business English");
                foreach (var order in businessOrders)
                {
                    Console.WriteLine($"Order #{order.OrderId}: User {order.UserId} - ${order.Price} - {order.Status}");
                }

                // 7. Пошук замовлень з фільтрами
                Console.WriteLine("\n--- Search: Completed orders for User 1 ---");
                var searchResults = await repository.SearchOrdersAsync(userId: 1, status: "Completed");
                foreach (var order in searchResults)
                {
                    Console.WriteLine($"Found: Order #{order.OrderId} - {order.CategoryName} - ${order.Price}");
                }

                // 8. Замовлення за останні 30 днів
                Console.WriteLine("\n--- Recent Orders (Last 30 days) ---");
                var recentOrders = await repository.GetRecentOrdersAsync(30);
                foreach (var order in recentOrders.Take(5))
                {
                    Console.WriteLine($"Recent: Order #{order.OrderId} - {order.CategoryName} ({order.OrderDate:yyyy-MM-dd})");
                }

                // 9. Статистика по категоріях
                Console.WriteLine("\n--- Category Statistics ---");
                var categoryStats = await repository.GetCategoryStatisticsAsync();
                foreach (var stat in categoryStats)
                {
                    Console.WriteLine($"📊 {stat.CategoryName}: {stat.OrderCount} orders | Revenue: ${stat.TotalRevenue} | Avg: ${stat.AveragePrice:F2}");
                }

                // 10. Статистика по статусах
                Console.WriteLine("\n--- Status Statistics ---");
                var statusStats = await repository.GetStatusStatisticsAsync();
                foreach (var stat in statusStats)
                {
                    Console.WriteLine($"📈 {stat.Status}: {stat.OrderCount} orders | Total: ${stat.TotalAmount}");
                }

                // 11. Загальна статистика
                Console.WriteLine("\n--- General Statistics ---");
                var generalStats = await repository.GetGeneralStatisticsAsync();
                Console.WriteLine($"Total Orders: {generalStats.TotalOrders}");
                Console.WriteLine($"Completed: {generalStats.CompletedOrders} | Pending: {generalStats.PendingOrders} | Cancelled: {generalStats.CancelledOrders}");
                Console.WriteLine($"Total Revenue: ${generalStats.TotalRevenue}");
                Console.WriteLine($"Average Order Value: ${generalStats.AverageOrderValue:F2}");
                Console.WriteLine($"Unique Customers: {generalStats.UniqueCustomers}");
                Console.WriteLine($"Unique Categories: {generalStats.UniqueCategories}");

                // 12. Топ користувачів
                Console.WriteLine("\n--- Top 3 Customers ---");
                var topCustomers = await repository.GetTopCustomersAsync(3);
                foreach (var customer in topCustomers)
                {
                    Console.WriteLine($"🏆 User {customer.UserId}: {customer.OrderCount} orders | Spent: ${customer.TotalSpent} | Avg: ${customer.AverageOrderValue:F2}");
                }

                // 13. Замовлення за період
                Console.WriteLine("\n--- Orders This Month ---");
                var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);
                var monthlyOrders = await repository.GetOrdersByDateRangeAsync(thisMonthStart, thisMonthEnd);
                foreach (var order in monthlyOrders.Take(3))
                {
                    Console.WriteLine($"This month: Order #{order.OrderId} - {order.CategoryName} ({order.OrderDate:yyyy-MM-dd})");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  Операції виконано успішно!");
                Console.WriteLine("  Order Service з повною аналітикою");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}