using MySql.Data.MySqlClient;

namespace UserManagementService
{
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            "Server=localhost;Database=flasheng_user_management;User=admin;Password=1234567890;";

        public static string ServerConnectionString =>
            "Server=localhost;User=admin;Password=1234567890;";

        /// <summary>
        /// Створити базу даних
        /// </summary>
        public static async Task EnsureDatabaseCreatedAsync()
        {
            using var connection = new MySqlConnection(ServerConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_user_management;";
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("✅ Database created");
        }

        /// <summary>
        /// Створити таблиці
        /// </summary>
        public static async Task EnsureTablesCreatedAsync()
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // 1. Таблиця UserProfiles
            var createUserProfilesTable = @"
                CREATE TABLE IF NOT EXISTS UserProfiles (
                    UserId INT AUTO_INCREMENT PRIMARY KEY,
                    Email VARCHAR(255) UNIQUE NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    FullName VARCHAR(255) NOT NULL,
                    Role VARCHAR(20) DEFAULT 'User',
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

            // 2. Таблиця UserSettings (1:1 зв'язок)
            var createUserSettingsTable = @"
                CREATE TABLE IF NOT EXISTS UserSettings (
                    SettingsId INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT UNIQUE NOT NULL,
                    Theme VARCHAR(20) DEFAULT 'Light',
                    Language VARCHAR(5) DEFAULT 'en',
                    NotificationsEnabled BOOLEAN DEFAULT TRUE,
                    FOREIGN KEY (UserId) REFERENCES UserProfiles(UserId) ON DELETE CASCADE
                );";

            var command = connection.CreateCommand();

            command.CommandText = createUserProfilesTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserProfiles' created");

            command.CommandText = createUserSettingsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserSettings' created (1:1 relationship)");

            // Додати збережувану процедуру
            await CreateStoredProcedureAsync(connection);

            // Додати тестові дані
            await SeedDataAsync(connection);
        }

        /// <summary>
        /// Створити збережувану процедуру
        /// </summary>
        private static async Task CreateStoredProcedureAsync(MySqlConnection connection)
        {
            var createUserWithSettingsProcedure = @"
                DROP PROCEDURE IF EXISTS CreateUserWithSettings;
                CREATE PROCEDURE CreateUserWithSettings(
                    IN p_Email VARCHAR(255),
                    IN p_Password VARCHAR(255),
                    IN p_FullName VARCHAR(255),
                    IN p_Theme VARCHAR(20),
                    IN p_Language VARCHAR(5),
                    OUT p_UserId INT
                )
                BEGIN
                    START TRANSACTION;
                    
                    INSERT INTO UserProfiles (Email, PasswordHash, FullName, IsActive)
                    VALUES (p_Email, p_Password, p_FullName, TRUE);
                    
                    SET p_UserId = LAST_INSERT_ID();
                    
                    INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled)
                    VALUES (p_UserId, p_Theme, p_Language, TRUE);
                    
                    COMMIT;
                END;";

            var command = connection.CreateCommand();
            command.CommandText = createUserWithSettingsProcedure;
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("✅ Stored procedure 'CreateUserWithSettings' created");
        }

        /// <summary>
        /// Додати тестові дані
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM UserProfiles;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                // Додати користувачів
                var seedUserData = @"
                    INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, IsActive) 
                    VALUES 
                        ('admin@flasheng.com', 'hashed_password_1', 'Admin User', 'Admin', TRUE),
                        ('john@flasheng.com', 'hashed_password_2', 'John Doe', 'User', TRUE),
                        ('jane@flasheng.com', 'hashed_password_3', 'Jane Smith', 'User', TRUE);
                ";

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedUserData;
                await seedCommand.ExecuteNonQueryAsync();

                // Додати налаштування
                var seedSettingsData = @"
                    INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled) 
                    VALUES 
                        (1, 'Dark', 'en', TRUE),
                        (2, 'Light', 'en', TRUE),
                        (3, 'Auto', 'uk', FALSE);
                ";

                seedCommand.CommandText = seedSettingsData;
                await seedCommand.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Test data added: 3 users with settings");
            }
        }
    }
}