using MySql.Data.MySqlClient;

namespace UsersService
{
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            "Server=localhost;Database=flasheng_users_simple;User=admin;Password=1234567890;";

        public static string ServerConnectionString =>
            "Server=localhost;User=admin;Password=1234567890;";

        /// <summary>
        /// Створити базу даних, якщо її не існує
        /// </summary>
        public static async Task EnsureDatabaseCreatedAsync()
        {
            using var connection = new MySqlConnection(ServerConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_users_simple;";

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Database 'flasheng_users_simple' ensured.");
        }

        /// <summary>
        /// Створити таблиці, якщо їх не існує
        /// </summary>
        public static async Task EnsureTablesCreatedAsync()
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Створення таблиці UserProfiles
            var createUserProfilesTable = @"
                CREATE TABLE IF NOT EXISTS UserProfiles (
                    UserId INT AUTO_INCREMENT PRIMARY KEY,
                    Email VARCHAR(255) UNIQUE NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    FullName VARCHAR(255) NOT NULL,
                    Role VARCHAR(20) DEFAULT 'User',
                    EnglishLevel VARCHAR(2) DEFAULT 'A1',
                    PreferredAIModel VARCHAR(20) DEFAULT 'GPT-3.5',
                    DailyGoal INT DEFAULT 10,
                    NotificationsEnabled BOOLEAN DEFAULT TRUE,
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    INDEX idx_email (Email),
                    INDEX idx_role (Role),
                    INDEX idx_active (IsActive)
                );";

            var command = connection.CreateCommand();
            command.CommandText = createUserProfilesTable;
            await command.ExecuteNonQueryAsync();

            // Додати початкові дані
            await SeedDataAsync(connection);

            Console.WriteLine("✅ Table 'UserProfiles' ensured.");
        }

        /// <summary>
        /// Додати початкові дані
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            // Перевірити, чи є дані
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM UserProfiles;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var seedUserData = @"
                    INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, EnglishLevel, PreferredAIModel, DailyGoal, NotificationsEnabled, IsActive) 
                    VALUES 
                        ('admin@flasheng.com', 'hashed_admin_password', 'Admin User', 'Admin', 'C1', 'GPT-4', 20, TRUE, TRUE),
                        ('user1@flasheng.com', 'hashed_user1_password', 'John Doe', 'User', 'B2', 'GPT-3.5', 15, TRUE, TRUE),
                        ('user2@flasheng.com', 'hashed_user2_password', 'Jane Smith', 'Premium', 'B1', 'GPT-4', 25, TRUE, TRUE);
                ";

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedUserData;
                await seedCommand.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Seed data added to UserProfiles.");
            }
        }
    }
}