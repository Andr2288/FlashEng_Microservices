using MySql.Data.MySqlClient;

namespace OrderService
{
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            "Server=localhost;Database=flasheng_orders;User=admin;Password=1234567890;";

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
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_orders;";

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Database 'flasheng_orders' ensured.");
        }

        /// <summary>
        /// Створити таблиці, якщо їх не існує
        /// </summary>
        public static async Task EnsureTablesCreatedAsync()
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Створення таблиці Orders
            var createOrdersTable = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    OrderId INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    CategoryName VARCHAR(100) NOT NULL,
                    Price DECIMAL(10,2) NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Pending',
                    OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    CompletedDate TIMESTAMP NULL,
                    INDEX idx_userid (UserId),
                    INDEX idx_status (Status),
                    INDEX idx_category (CategoryName),
                    INDEX idx_orderdate (OrderDate)
                );";

            var command = connection.CreateCommand();
            command.CommandText = createOrdersTable;
            await command.ExecuteNonQueryAsync();

            // Додати початкові дані
            await SeedDataAsync(connection);

            Console.WriteLine("✅ Table 'Orders' ensured.");
        }

        /// <summary>
        /// Додати початкові дані
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            // Перевірити, чи є дані
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Orders;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var seedOrderData = @"
                    INSERT INTO Orders (UserId, CategoryName, Price, Status, OrderDate, CompletedDate) 
                    VALUES 
                        (1, 'Business English', 29.99, 'Completed', '2024-10-01 10:00:00', '2024-10-01 10:05:00'),
                        (1, 'Travel Phrases', 19.99, 'Completed', '2024-10-15 14:30:00', '2024-10-15 14:35:00'),
                        (2, 'Business English', 29.99, 'Pending', '2024-11-01 09:15:00', NULL),
                        (3, 'Advanced Grammar', 39.99, 'Completed', '2024-11-02 16:20:00', '2024-11-02 16:25:00'),
                        (2, 'Travel Phrases', 19.99, 'Cancelled', '2024-11-03 11:45:00', NULL);
                ";

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedOrderData;
                await seedCommand.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Seed data added to Orders.");
            }
        }
    }
}