using UsersService;

namespace FlashEngUsers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Users Service (СПРОЩЕНО)");
            Console.WriteLine("  UserProfile + Orders + ADO.NET + Dapper");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних, якщо її не існує
                await DatabaseConfig.EnsureDatabaseCreatedAsync();

                // Створюємо таблиці, якщо їх не існує
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new UserRepository();

                // 1. Показати всіх користувачів
                Console.WriteLine("--- All Users ---");
                var users = await repository.GetAllUsersAsync();
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.UserId} | {user.FullName} | {user.Email} | Role: {user.Role} | Level: {user.EnglishLevel}");
                }

                // 2. Показати користувача по ID
                Console.WriteLine("\n--- User Info (ID = 1) ---");
                var user1 = await repository.GetUserByIdAsync(1);
                if (user1 != null)
                {
                    Console.WriteLine($"Name: {user1.FullName}");
                    Console.WriteLine($"Email: {user1.Email}");
                    Console.WriteLine($"Role: {user1.Role}");
                    Console.WriteLine($"English Level: {user1.EnglishLevel}");
                    Console.WriteLine($"Daily Goal: {user1.DailyGoal}");
                    Console.WriteLine($"Notifications: {user1.NotificationsEnabled}");
                }

                // 3. Створити нового користувача
                Console.WriteLine("\n--- Creating New User ---");
                string testEmail = $"test_user_{DateTime.Now:yyyyMMddHHmmss}@flasheng.com";
                try
                {
                    int newUserId = await repository.CreateUserAsync(
                        testEmail,
                        "hashed_password_123",
                        "Test User",
                        "B1",
                        "User"
                    );
                    Console.WriteLine($"New user created with ID: {newUserId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"User creation failed: {ex.Message}");
                }

                // 4. Створити замовлення
                Console.WriteLine("\n--- Creating Orders ---");
                try
                {
                    int orderId1 = await repository.CreateOrderAsync(1, "Business English", 29.99m);
                    int orderId2 = await repository.CreateOrderAsync(1, "Travel Phrases", 19.99m);

                    Console.WriteLine($"Order 1 created with ID: {orderId1}");
                    Console.WriteLine($"Order 2 created with ID: {orderId2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Order creation failed: {ex.Message}");
                }

                // 5. Показати замовлення користувача
                Console.WriteLine("\n--- User Orders (User ID = 1) ---");
                var orders = await repository.GetUserOrdersAsync(1);
                foreach (var order in orders)
                {
                    Console.WriteLine($"Order #{order.OrderId}: {order.CategoryName} - ${order.Price} - {order.Status}");
                }

                // 6. Оновити статус замовлення
                if (orders.Any())
                {
                    Console.WriteLine("\n--- Updating Order Status ---");
                    bool updated = await repository.UpdateOrderStatusAsync(orders.First().OrderId, "Completed");
                    Console.WriteLine($"Order status updated: {updated}");
                }

                // 7. Оновити профіль користувача
                Console.WriteLine("\n--- Updating User Profile (ID = 1) ---");
                int rowsAffected = await repository.UpdateUserProfileAsync(
                    userId: 1,
                    englishLevel: "B2",
                    dailyGoal: 20,
                    notificationsEnabled: true
                );
                Console.WriteLine($"Profile updated, rows affected: {rowsAffected}");

                // 8. Статистика користувачів
                Console.WriteLine("\n--- User Statistics ---");
                var stats = await repository.GetUserStatisticsAsync();
                foreach (var stat in stats)
                {
                    Console.WriteLine($"Role: {stat.Role} | Total: {stat.UserCount} | Active: {stat.ActiveUsers}");
                }

                // 9. Всі замовлення (для адміна)
                Console.WriteLine("\n--- All Orders (Admin View) ---");
                var allOrders = await repository.GetAllOrdersAsync();
                foreach (var order in allOrders.Take(5)) // показати перші 5
                {
                    Console.WriteLine($"Order #{order.OrderId}: User {order.UserId} | {order.CategoryName} | ${order.Price} | {order.Status}");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  Операції виконано успішно!");
                Console.WriteLine("  Спрощена модель: UserProfile + Orders");
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