using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Configuration
{
    public static class DatabaseConfig
    {
        public static async Task EnsureDatabasesCreatedAsync(string serverConnectionString)
        {
            await using var connection = new MySqlConnection(serverConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();

            // Створення бази для користувачів
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_users;";
            await command.ExecuteNonQueryAsync();

            // Створення бази для флешкарток
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_flashcards;";
            await command.ExecuteNonQueryAsync();

            // Створення бази для замовлень
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_orders;";
            await command.ExecuteNonQueryAsync();
        }

        public static async Task CreateUsersTablesAsync(string connectionString)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

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

            command.CommandText = createUserSettingsTable;
            await command.ExecuteNonQueryAsync();

            // Додати тестові дані якщо їх немає
            await SeedUsersDataAsync(connection);
        }

        public static async Task CreateFlashcardsTablesAsync(string connectionString)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var createFlashcardsTable = @"
            CREATE TABLE IF NOT EXISTS Flashcards (
                FlashcardId INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                Category VARCHAR(100) NOT NULL,
                EnglishWord VARCHAR(200) NOT NULL,
                Translation VARCHAR(200) NOT NULL,
                Definition TEXT,
                ExampleSentence TEXT,
                Pronunciation VARCHAR(100),
                AudioUrl VARCHAR(500),
                ImageUrl VARCHAR(500),
                Difficulty VARCHAR(20) DEFAULT 'Medium',
                IsPublic BOOLEAN DEFAULT FALSE,
                Price DECIMAL(10,2),
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                INDEX idx_userid (UserId),
                INDEX idx_category (Category),
                INDEX idx_word (EnglishWord)
            );";

            var command = connection.CreateCommand();
            command.CommandText = createFlashcardsTable;
            await command.ExecuteNonQueryAsync();

            await SeedFlashcardsDataAsync(connection);
        }

        public static async Task CreateOrdersTablesAsync(string connectionString)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var createProductsTable = @"
            CREATE TABLE IF NOT EXISTS Products (
                ProductId INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Price DECIMAL(10,2) NOT NULL,
                IsAvailable BOOLEAN DEFAULT TRUE,
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";

            var createOrdersTable = @"
            CREATE TABLE IF NOT EXISTS Orders (
                OrderId INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
                Status VARCHAR(20) DEFAULT 'Pending',
                OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";

            var createOrderItemsTable = @"
            CREATE TABLE IF NOT EXISTS OrderItems (
                OrderItemId INT AUTO_INCREMENT PRIMARY KEY,
                OrderId INT NOT NULL,
                ProductId INT NOT NULL,
                Quantity INT NOT NULL,
                UnitPrice DECIMAL(10,2) NOT NULL,
                LineTotal DECIMAL(10,2) NOT NULL,
                FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE RESTRICT
            );";

            var createPaymentsTable = @"
            CREATE TABLE IF NOT EXISTS Payments (
                PaymentId INT AUTO_INCREMENT PRIMARY KEY,
                OrderId INT NOT NULL UNIQUE,
                Amount DECIMAL(10,2) NOT NULL,
                PaymentMethod VARCHAR(50) NOT NULL,
                Status VARCHAR(20) DEFAULT 'Pending',
                PaymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
            );";

            var command = connection.CreateCommand();
            command.CommandText = createProductsTable;
            await command.ExecuteNonQueryAsync();

            command.CommandText = createOrdersTable;
            await command.ExecuteNonQueryAsync();

            command.CommandText = createOrderItemsTable;
            await command.ExecuteNonQueryAsync();

            command.CommandText = createPaymentsTable;
            await command.ExecuteNonQueryAsync();

            await CreateOrderStoredProceduresAsync(connection);
            await SeedOrdersDataAsync(connection);
        }

        private static async Task CreateOrderStoredProceduresAsync(MySqlConnection connection)
        {
            var createOrderWithItemsProcedure = @"
            DROP PROCEDURE IF EXISTS CreateOrderWithItems;
            CREATE PROCEDURE CreateOrderWithItems(
                IN p_UserId INT,
                IN p_ProductIds TEXT,
                IN p_Quantities TEXT,
                OUT p_OrderId INT
            )
            BEGIN
                DECLARE v_ProductId INT;
                DECLARE v_Quantity INT;
                DECLARE v_UnitPrice DECIMAL(10,2);
                DECLARE v_LineTotal DECIMAL(10,2);
                DECLARE v_TotalAmount DECIMAL(10,2) DEFAULT 0;
                DECLARE idx INT DEFAULT 1;
                
                DECLARE EXIT HANDLER FOR SQLEXCEPTION
                BEGIN
                    ROLLBACK;
                    RESIGNAL;
                END;
                
                START TRANSACTION;
                
                INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate)
                VALUES (p_UserId, 0, 'Pending', NOW());
                
                SET p_OrderId = LAST_INSERT_ID();
                
                WHILE idx <= (CHAR_LENGTH(p_ProductIds) - CHAR_LENGTH(REPLACE(p_ProductIds, ',', '')) + 1) DO
                    SET v_ProductId = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(p_ProductIds, ',', idx), ',', -1) AS UNSIGNED);
                    SET v_Quantity = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(p_Quantities, ',', idx), ',', -1) AS UNSIGNED);
                    
                    SELECT Price INTO v_UnitPrice FROM Products WHERE ProductId = v_ProductId AND IsAvailable = TRUE;
                    
                    IF v_UnitPrice IS NULL THEN
                        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Product not found or not available';
                    END IF;
                    
                    SET v_LineTotal = v_UnitPrice * v_Quantity;
                    SET v_TotalAmount = v_TotalAmount + v_LineTotal;
                    
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                    VALUES (p_OrderId, v_ProductId, v_Quantity, v_UnitPrice, v_LineTotal);
                    
                    SET idx = idx + 1;
                END WHILE;
                
                UPDATE Orders SET TotalAmount = v_TotalAmount WHERE OrderId = p_OrderId;
                
                COMMIT;
            END;";

            var command = connection.CreateCommand();
            command.CommandText = createOrderWithItemsProcedure;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task SeedUsersDataAsync(MySqlConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM UserProfiles;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var seedData = @"
                INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, IsActive) 
                VALUES 
                    ('admin@flasheng.com', 'hashed_password_1', 'Admin User', 'Admin', TRUE),
                    ('john@flasheng.com', 'hashed_password_2', 'John Doe', 'User', TRUE),
                    ('jane@flasheng.com', 'hashed_password_3', 'Jane Smith', 'User', TRUE);

                INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled) 
                VALUES 
                    (1, 'Dark', 'en', TRUE),
                    (2, 'Light', 'en', TRUE),
                    (3, 'Auto', 'uk', FALSE);
            ";

                var command = connection.CreateCommand();
                command.CommandText = seedData;
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task SeedFlashcardsDataAsync(MySqlConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Flashcards;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var seedData = @"
                INSERT INTO Flashcards (UserId, Category, EnglishWord, Translation, Definition, ExampleSentence, Pronunciation, Difficulty, IsPublic, Price) 
                VALUES 
                    (1, 'Food', 'apple', 'яблуко', 'A round fruit with red or green skin', 'I eat an apple every day.', '/ˈæp.əl/', 'Easy', FALSE, NULL),
                    (1, 'Food', 'bread', 'хліб', 'A basic food made from flour and water', 'I buy fresh bread every morning.', '/bred/', 'Easy', FALSE, NULL),
                    (1, 'Business English', 'meeting', 'зустріч', 'A gathering of people for discussion', 'We have a meeting at 3 PM.', '/ˈmiː.tɪŋ/', 'Medium', TRUE, 29.99),
                    (1, 'Business English', 'presentation', 'презентація', 'A speech or talk in which something is shown', 'I need to prepare a presentation for tomorrow.', '/ˌprez.ənˈteɪ.ʃən/', 'Hard', TRUE, 29.99),
                    (1, 'Travel Phrases', 'airport', 'аеропорт', 'A place where planes take off and land', 'I need to be at the airport two hours early.', '/ˈeə.pɔːt/', 'Easy', TRUE, 19.99);
            ";

                var command = connection.CreateCommand();
                command.CommandText = seedData;
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task SeedOrdersDataAsync(MySqlConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Products;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var seedData = @"
                INSERT INTO Products (Name, Price, IsAvailable) VALUES 
                    ('Business English Course', 29.99, TRUE),
                    ('Travel Phrases Pack', 19.99, TRUE),
                    ('Advanced Grammar', 39.99, TRUE),
                    ('IELTS Preparation', 49.99, TRUE);

                INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate) VALUES 
                    (1, 49.98, 'Completed', '2024-10-01 10:00:00'),
                    (2, 39.99, 'Pending', '2024-11-01 14:30:00');

                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal) VALUES 
                    (1, 1, 1, 29.99, 29.99),
                    (1, 2, 1, 19.99, 19.99),
                    (2, 3, 1, 39.99, 39.99);

                INSERT INTO Payments (OrderId, Amount, PaymentMethod, Status, PaymentDate) VALUES 
                    (1, 49.98, 'Card', 'Completed', '2024-10-01 10:05:00');
            ";

                var command = connection.CreateCommand();
                command.CommandText = seedData;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
