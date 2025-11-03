using UsersService;

namespace FlashEngUsers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Users Service (ADO.NET + Dapper)");
            Console.WriteLine("  With ASYNC + TRANSACTIONS");
            Console.WriteLine("===========================================\n");

            try
            {
                var repository = new UserRepository();

                // 1. Показати всіх користувачів (ASYNC)
                Console.WriteLine("--- All Users ---");
                var users = await repository.GetAllUsersAsync();
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.UserId} | {user.FullName} | {user.Email} | Active: {user.IsActive}");
                }

                // 2. Показати інформацію про користувача (ASYNC)
                Console.WriteLine("\n--- User Info (ID = 1) ---");
                var userInfo = await repository.GetUserInfoAsync(1);
                if (userInfo != null)
                {
                    Console.WriteLine($"Name: {userInfo.FullName}");
                    Console.WriteLine($"Email: {userInfo.Email}");
                    Console.WriteLine($"English Level: {userInfo.EnglishLevel}");
                    Console.WriteLine($"AI Model: {userInfo.PreferredAIModel}");
                    Console.WriteLine($"Daily Goal: {userInfo.DailyGoal}");
                    Console.WriteLine($"Roles: {userInfo.Roles}");
                }

                // 3. Створити користувача через ЗБЕРЕЖУВАНУ ПРОЦЕДУРУ (якщо не існує)
                Console.WriteLine("\n--- Creating User via Stored Procedure ---");
                string testEmail1 = $"procedure_user_{DateTime.Now:yyyyMMddHHmmss}@flasheng.com";
                try
                {
                    int newUserId1 = await repository.CreateUserAsync(
                        testEmail1,
                        "hashed_password_proc",
                        "User via Procedure",
                        "A2"
                    );
                    Console.WriteLine($"New user created via procedure with ID: {newUserId1}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"User creation failed: {ex.Message}");
                }

                // 4. Створити користувача з ТРАНЗАКЦІЄЮ (якщо не існує)
                Console.WriteLine("\n--- Creating User with TRANSACTION ---");
                string testEmail2 = $"transaction_user_{DateTime.Now:yyyyMMddHHmmss}@flasheng.com";
                try
                {
                    int newUserId2 = await repository.CreateUserWithTransactionAsync(
                        testEmail2,
                        "hashed_password_trans",
                        "User with Transaction",
                        "B1"
                    );
                    Console.WriteLine($"New user created with transaction, ID: {newUserId2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Transaction failed: {ex.Message}");
                }

                // 5. Оновити профіль користувача (ASYNC)
                Console.WriteLine("\n--- Updating User Profile (ID = 2) ---");
                int rowsAffected = await repository.UpdateUserProfileAsync(
                    userId: 2,
                    englishLevel: "B2",
                    dailyGoal: 25,
                    notificationsEnabled: true
                );
                Console.WriteLine($"Rows affected: {rowsAffected}");

                // 6. Показати всі ролі (ASYNC)
                Console.WriteLine("\n--- All Roles ---");
                var roles = await repository.GetAllRolesAsync();
                foreach (var role in roles)
                {
                    Console.WriteLine($"ID: {role.RoleId} | {role.RoleName} | {role.Description}");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  All operations completed successfully!");
                Console.WriteLine("  Used: ASYNC/AWAIT + TRANSACTIONS");
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