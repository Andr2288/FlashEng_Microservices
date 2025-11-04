using MySql.Data.MySqlClient;

namespace OrderService
{
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            "Server=localhost;Database=flasheng_simple_orders;User=admin;Password=1234567890;";

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
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_simple_orders;";

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Database 'flasheng_simple_orders' created.");
        }

        /// <summary>
        /// Створити таблиці
        /// </summary>
        public static async Task EnsureTablesCreatedAsync()
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // 1. Таблиця Products
            var createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    ProductId INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Price DECIMAL(10,2) NOT NULL CHECK (Price > 0),
                    IsAvailable BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_name (Name),
                    INDEX idx_price (Price)
                );";

            // 2. Таблиця Orders
            var createOrdersTable = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    OrderId INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
                    Status VARCHAR(20) DEFAULT 'Pending',
                    OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_userid (UserId),
                    INDEX idx_status (Status)
                );";

            // 3. Таблиця OrderItems (M:N між Orders та Products)
            var createOrderItemsTable = @"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    OrderItemId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL CHECK (Quantity > 0),
                    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice >= 0),
                    LineTotal DECIMAL(10,2) NOT NULL CHECK (LineTotal >= 0),
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE RESTRICT,
                    INDEX idx_order (OrderId),
                    INDEX idx_product (ProductId)
                );";

            // 4. Таблиця Payments (1:1 з Orders)
            var createPaymentsTable = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    PaymentId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL UNIQUE,
                    Amount DECIMAL(10,2) NOT NULL CHECK (Amount > 0),
                    PaymentMethod VARCHAR(50) NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Pending',
                    PaymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
                );";

            var command = connection.CreateCommand();

            // Створюємо таблиці в правильному порядку
            command.CommandText = createProductsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Products' created.");

            command.CommandText = createOrdersTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Orders' created.");

            command.CommandText = createOrderItemsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'OrderItems' created.");

            command.CommandText = createPaymentsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Payments' created.");

            // Створити збережувані процедури
            await CreateStoredProceduresAsync(connection);

            // Додати тестові дані
            await SeedDataAsync(connection);
        }

        /// <summary>
        /// Створити збережувані процедури
        /// </summary>
        private static async Task CreateStoredProceduresAsync(MySqlConnection connection)
        {
            // 1. Процедура створення замовлення з товарами
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
                    
                    -- Створити замовлення
                    INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate)
                    VALUES (p_UserId, 0, 'Pending', NOW());
                    
                    SET p_OrderId = LAST_INSERT_ID();
                    
                    -- Обробити позиції замовлення
                    WHILE idx <= (CHAR_LENGTH(p_ProductIds) - CHAR_LENGTH(REPLACE(p_ProductIds, ',', '')) + 1) DO
                        SET v_ProductId = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(p_ProductIds, ',', idx), ',', -1) AS UNSIGNED);
                        SET v_Quantity = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(p_Quantities, ',', idx), ',', -1) AS UNSIGNED);
                        
                        -- Отримати ціну продукту
                        SELECT Price INTO v_UnitPrice FROM Products WHERE ProductId = v_ProductId AND IsAvailable = TRUE;
                        
                        IF v_UnitPrice IS NULL THEN
                            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Product not found or not available';
                        END IF;
                        
                        SET v_LineTotal = v_UnitPrice * v_Quantity;
                        SET v_TotalAmount = v_TotalAmount + v_LineTotal;
                        
                        -- Додати позицію
                        INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                        VALUES (p_OrderId, v_ProductId, v_Quantity, v_UnitPrice, v_LineTotal);
                        
                        SET idx = idx + 1;
                    END WHILE;
                    
                    -- Оновити загальну суму замовлення
                    UPDATE Orders SET TotalAmount = v_TotalAmount WHERE OrderId = p_OrderId;
                    
                    COMMIT;
                END;";

            // 2. Процедура отримання деталей замовлення
            var getOrderDetailsProcedure = @"
                DROP PROCEDURE IF EXISTS GetOrderDetails;
                CREATE PROCEDURE GetOrderDetails(IN p_OrderId INT)
                BEGIN
                    -- Основна інформація про замовлення
                    SELECT * FROM Orders WHERE OrderId = p_OrderId;
                    
                    -- Позиції замовлення з назвами продуктів
                    SELECT 
                        oi.OrderItemId,
                        oi.ProductId,
                        p.Name as ProductName,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.LineTotal
                    FROM OrderItems oi
                    JOIN Products p ON oi.ProductId = p.ProductId
                    WHERE oi.OrderId = p_OrderId;
                    
                    -- Інформація про платіж
                    SELECT * FROM Payments WHERE OrderId = p_OrderId;
                END;";

            // 3. Процедура зміни статусу замовлення
            var updateOrderStatusProcedure = @"
                DROP PROCEDURE IF EXISTS UpdateOrderStatus;
                CREATE PROCEDURE UpdateOrderStatus(
                    IN p_OrderId INT,
                    IN p_Status VARCHAR(20)
                )
                BEGIN
                    UPDATE Orders SET Status = p_Status WHERE OrderId = p_OrderId;
                    
                    IF p_Status = 'Completed' THEN
                        INSERT INTO Payments (OrderId, Amount, PaymentMethod, Status, PaymentDate)
                        SELECT OrderId, TotalAmount, 'Card', 'Completed', NOW()
                        FROM Orders 
                        WHERE OrderId = p_OrderId
                        AND NOT EXISTS (SELECT 1 FROM Payments WHERE OrderId = p_OrderId);
                    END IF;
                END;";

            // 4. Процедура отримання статистики
            var getOrderStatisticsProcedure = @"
                DROP PROCEDURE IF EXISTS GetOrderStatistics;
                CREATE PROCEDURE GetOrderStatistics()
                BEGIN
                    SELECT 
                        COUNT(*) as TotalOrders,
                        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as CompletedOrders,
                        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingOrders,
                        SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) as CancelledOrders,
                        SUM(CASE WHEN Status = 'Completed' THEN TotalAmount ELSE 0 END) as TotalRevenue,
                        AVG(CASE WHEN Status = 'Completed' THEN TotalAmount END) as AverageOrderValue
                    FROM Orders;
                END;";

            var command = connection.CreateCommand();

            command.CommandText = createOrderWithItemsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Procedure 'CreateOrderWithItems' created.");

            command.CommandText = getOrderDetailsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Procedure 'GetOrderDetails' created.");

            command.CommandText = updateOrderStatusProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Procedure 'UpdateOrderStatus' created.");

            command.CommandText = getOrderStatisticsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Procedure 'GetOrderStatistics' created.");
        }

        /// <summary>
        /// Додати тестові дані
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Products;";
            var productCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (productCount == 0)
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

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedData;
                await seedCommand.ExecuteNonQueryAsync();
                Console.WriteLine("✅ Test data added.");
            }
        }
    }
}