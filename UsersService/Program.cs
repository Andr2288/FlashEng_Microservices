using UsersService;

namespace FlashEngUsers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Users Service (БЕЗ ORDERS)");
            Console.WriteLine("  UserProfile + ADO.NET + Dapper");
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

                // 4. Оновити профіль користувача
                Console.WriteLine("\n--- Updating User Profile (ID = 1) ---");
                int rowsAffected = await repository.UpdateUserProfileAsync(
                    userId: 1,
                    englishLevel: "B2",
                    dailyGoal: 20,
                    notificationsEnabled: true
                );
                Console.WriteLine($"Profile updated, rows affected: {rowsAffected}");

                // 5. Статистика користувачів
                Console.WriteLine("\n--- User Statistics ---");
                var stats = await repository.GetUserStatisticsAsync();
                foreach (var stat in stats)
                {
                    Console.WriteLine($"Role: {stat.Role} | Total: {stat.UserCount} | Active: {stat.ActiveUsers}");
                }

                // 6. Пошук користувача по email
                Console.WriteLine("\n--- Find User by Email ---");
                var foundUser = await repository.GetUserByEmailAsync("user1@flasheng.com");
                if (foundUser != null)
                {
                    Console.WriteLine($"Found: {foundUser.FullName} ({foundUser.Email})");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  Операції виконано успішно!");
                Console.WriteLine("  Спрощена модель: UserProfile (БЕЗ ORDERS)");
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