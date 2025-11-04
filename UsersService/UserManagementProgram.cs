using UserManagementService;

namespace FlashEngUserManagement
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - User Management Service");
            Console.WriteLine("  Спрощена версія для навчання");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних та таблиці
                await DatabaseConfig.EnsureDatabaseCreatedAsync();
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new UserManagementRepository();

                Console.WriteLine("\n--- 📋 ВСІ КОРИСТУВАЧІ ---");
                var users = await repository.GetAllUsersAsync();
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.UserId} | {user.FullName} ({user.Email}) | Role: {user.Role}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}