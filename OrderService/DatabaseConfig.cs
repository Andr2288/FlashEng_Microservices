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

            // 1. Створення таблиці Products
            var createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    ProductId INT AUTO_INCREMENT PRIMARY KEY,
                    CategoryName VARCHAR(100) NOT NULL,
                    Price DECIMAL(10,2) NOT NULL,
                    Description TEXT,
                    IsAvailable BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_category (CategoryName),
                    INDEX idx_price (Price),
                    INDEX idx_available (IsAvailable)
                );";

            // 2. Створення таблиці Orders
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

            // 3. Створення таблиці OrderItems (M:N між Orders та Products)
            var createOrderItemsTable = @"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    OrderItemId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL DEFAULT 1,
                    UnitPrice DECIMAL(10,2) NOT NULL,
                    LineTotal DECIMAL(10,2) NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE RESTRICT,
                    INDEX idx_order (OrderId),
                    INDEX idx_product (ProductId),
                    CONSTRAINT chk_quantity CHECK (Quantity > 0),
                    CONSTRAINT chk_unitprice CHECK (UnitPrice >= 0),
                    CONSTRAINT chk_linetotal CHECK (LineTotal >= 0)
                );";

            // 4. Створення таблиці Payments (1:1 з Orders)
            var createPaymentsTable = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    PaymentId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL UNIQUE,
                    Amount DECIMAL(10,2) NOT NULL,
                    PaymentMethod VARCHAR(50) NOT NULL,
                    TransactionId VARCHAR(100) UNIQUE,
                    Status VARCHAR(20) DEFAULT 'Pending',
                    PaymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                    INDEX idx_transaction (TransactionId),
                    INDEX idx_status (Status),
                    INDEX idx_method (PaymentMethod),
                    CONSTRAINT chk_amount CHECK (Amount > 0)
                );";

            var command = connection.CreateCommand();

            // Створюємо таблиці в правильному порядку (через FK залежності)
            command.CommandText = createProductsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Products' ensured.");

            command.CommandText = createOrdersTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Orders' ensured.");

            command.CommandText = createOrderItemsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'OrderItems' ensured.");

            command.CommandText = createPaymentsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Payments' ensured.");

            // Створити збережувані процедури
            await CreateStoredProceduresAsync(connection);

            // Додати початкові дані
            await SeedDataAsync(connection);
        }

        /// <summary>
        /// Створити збережувані процедури
        /// </summary>
        private static async Task CreateStoredProceduresAsync(MySqlConnection connection)
        {
            // 1. Процедура створення замовлення з позиціями
            var createOrderWithItemsProcedure = @"
                DROP PROCEDURE IF EXISTS CreateOrderWithItems;
                CREATE PROCEDURE CreateOrderWithItems(
                    IN p_UserId INT,
                    IN p_CategoryName VARCHAR(100),
                    IN p_ProductIds TEXT,
                    IN p_Quantities TEXT,
                    IN p_PaymentMethod VARCHAR(50),
                    IN p_TransactionId VARCHAR(100),
                    OUT p_OrderId INT,
                    OUT p_TotalAmount DECIMAL(10,2)
                )
                BEGIN
                    DECLARE v_ProductId INT;
                    DECLARE v_Quantity INT;
                    DECLARE v_UnitPrice DECIMAL(10,2);
                    DECLARE v_LineTotal DECIMAL(10,2);
                    DECLARE v_TotalAmount DECIMAL(10,2) DEFAULT 0;
                    DECLARE done INT DEFAULT FALSE;
                    DECLARE idx INT DEFAULT 1;
                    
                    DECLARE EXIT HANDLER FOR SQLEXCEPTION
                    BEGIN
                        ROLLBACK;
                        RESIGNAL;
                    END;
                    
                    START TRANSACTION;
                    
                    -- Створити замовлення
                    INSERT INTO Orders (UserId, CategoryName, Price, Status, OrderDate)
                    VALUES (p_UserId, p_CategoryName, 0, 'Pending', NOW());
                    
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
                    UPDATE Orders SET Price = v_TotalAmount WHERE OrderId = p_OrderId;
                    
                    -- Створити запис платежу
                    INSERT INTO Payments (OrderId, Amount, PaymentMethod, TransactionId, Status, PaymentDate)
                    VALUES (p_OrderId, v_TotalAmount, p_PaymentMethod, p_TransactionId, 'Pending', NOW());
                    
                    SET p_TotalAmount = v_TotalAmount;
                    
                    COMMIT;
                END;";

            // 2. Процедура підтвердження платежу
            var confirmPaymentProcedure = @"
                DROP PROCEDURE IF EXISTS ConfirmPayment;
                CREATE PROCEDURE ConfirmPayment(
                    IN p_OrderId INT,
                    IN p_TransactionId VARCHAR(100)
                )
                BEGIN
                    DECLARE EXIT HANDLER FOR SQLEXCEPTION
                    BEGIN
                        ROLLBACK;
                        RESIGNAL;
                    END;
                    
                    START TRANSACTION;
                    
                    -- Оновити статус платежу
                    UPDATE Payments 
                    SET Status = 'Completed', TransactionId = p_TransactionId
                    WHERE OrderId = p_OrderId;
                    
                    -- Оновити статус замовлення
                    UPDATE Orders 
                    SET Status = 'Completed', CompletedDate = NOW()
                    WHERE OrderId = p_OrderId;
                    
                    COMMIT;
                END;";

            // 3. Процедура скасування замовлення
            var cancelOrderProcedure = @"
                DROP PROCEDURE IF EXISTS CancelOrder;
                CREATE PROCEDURE CancelOrder(
                    IN p_OrderId INT,
                    IN p_Reason VARCHAR(255)
                )
                BEGIN
                    UPDATE Orders SET Status = 'Cancelled' WHERE OrderId = p_OrderId;
                    UPDATE Payments SET Status = 'Failed' WHERE OrderId = p_OrderId;
                    
                    SELECT CONCAT('Order ', p_OrderId, ' cancelled. Reason: ', p_Reason) as Message;
                END;";

            // 4. Процедура отримання повної інформації про замовлення
            var getOrderDetailsProcedure = @"
                DROP PROCEDURE IF EXISTS GetOrderDetails;
                CREATE PROCEDURE GetOrderDetails(IN p_OrderId INT)
                BEGIN
                    -- Основна інформація про замовлення
                    SELECT * FROM Orders WHERE OrderId = p_OrderId;
                    
                    -- Позиції замовлення з інформацією про продукти
                    SELECT 
                        oi.OrderItemId,
                        oi.ProductId,
                        p.CategoryName,
                        p.Description,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.LineTotal
                    FROM OrderItems oi
                    JOIN Products p ON oi.ProductId = p.ProductId
                    WHERE oi.OrderId = p_OrderId;
                    
                    -- Інформація про платіж
                    SELECT * FROM Payments WHERE OrderId = p_OrderId;
                END;";

            // 5. Процедура статистики за період
            var getOrderStatisticsProcedure = @"
                DROP PROCEDURE IF EXISTS GetOrderStatistics;
                CREATE PROCEDURE GetOrderStatistics(
                    IN p_DateFrom DATE,
                    IN p_DateTo DATE
                )
                BEGIN
                    SELECT 
                        o.CategoryName,
                        COUNT(*) as OrderCount,
                        SUM(CASE WHEN o.Status = 'Completed' THEN o.Price ELSE 0 END) as TotalRevenue,
                        AVG(CASE WHEN o.Status = 'Completed' THEN o.Price ELSE NULL END) as AverageOrderValue,
                        SUM(CASE WHEN o.Status = 'Completed' THEN 1 ELSE 0 END) as CompletedOrders,
                        SUM(CASE WHEN o.Status = 'Pending' THEN 1 ELSE 0 END) as PendingOrders,
                        SUM(CASE WHEN o.Status = 'Cancelled' THEN 1 ELSE 0 END) as CancelledOrders
                    FROM Orders o
                    WHERE DATE(o.OrderDate) BETWEEN p_DateFrom AND p_DateTo
                    GROUP BY o.CategoryName
                    ORDER BY TotalRevenue DESC;
                END;";

            // 6. Процедура топ продуктів
            var getTopProductsProcedure = @"
                DROP PROCEDURE IF EXISTS GetTopProducts;
                CREATE PROCEDURE GetTopProducts(IN p_Limit INT)
                BEGIN
                    SELECT 
                        p.ProductId,
                        p.CategoryName,
                        p.Description,
                        p.Price,
                        SUM(oi.Quantity) as TotalSold,
                        SUM(oi.LineTotal) as TotalRevenue,
                        COUNT(DISTINCT oi.OrderId) as OrderCount
                    FROM Products p
                    JOIN OrderItems oi ON p.ProductId = oi.ProductId
                    JOIN Orders o ON oi.OrderId = o.OrderId
                    WHERE o.Status = 'Completed'
                    GROUP BY p.ProductId, p.CategoryName, p.Description, p.Price
                    ORDER BY TotalSold DESC
                    LIMIT p_Limit;
                END;";

            var command = connection.CreateCommand();

            command.CommandText = createOrderWithItemsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'CreateOrderWithItems' created.");

            command.CommandText = confirmPaymentProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'ConfirmPayment' created.");

            command.CommandText = cancelOrderProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'CancelOrder' created.");

            command.CommandText = getOrderDetailsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'GetOrderDetails' created.");

            command.CommandText = getOrderStatisticsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'GetOrderStatistics' created.");

            command.CommandText = getTopProductsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'GetTopProducts' created.");
        }

        /// <summary>
        /// Додати початкові дані
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            // Перевірити, чи є дані в Products
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Products;";
            var productCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (productCount == 0)
            {
                // Додати продукти
                var seedProductData = @"
                    INSERT INTO Products (CategoryName, Price, Description, IsAvailable) 
                    VALUES 
                        ('Business English', 29.99, 'Professional business vocabulary and phrases', TRUE),
                        ('Travel Phrases', 19.99, 'Essential phrases for travelers', TRUE),
                        ('Advanced Grammar', 39.99, 'Complex grammar rules and exercises', TRUE),
                        ('IELTS Preparation', 49.99, 'Complete IELTS exam preparation course', TRUE),
                        ('Daily Conversation', 24.99, 'Everyday English conversations', TRUE);
                ";

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedProductData;
                await seedCommand.ExecuteNonQueryAsync();
                Console.WriteLine("✅ Seed data added to Products.");

                // Перевірити, чи є дані в Orders
                checkCommand.CommandText = "SELECT COUNT(*) FROM Orders;";
                var orderCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                if (orderCount == 0)
                {
                    // Додати замовлення
                    var seedOrderData = @"
                        INSERT INTO Orders (UserId, CategoryName, Price, Status, OrderDate, CompletedDate) 
                        VALUES 
                            (1, 'Business English', 29.99, 'Completed', '2024-10-01 10:00:00', '2024-10-01 10:05:00'),
                            (1, 'Travel Phrases', 19.99, 'Completed', '2024-10-15 14:30:00', '2024-10-15 14:35:00'),
                            (2, 'Advanced Grammar', 39.99, 'Pending', '2024-11-01 09:15:00', NULL),
                            (3, 'IELTS Preparation', 49.99, 'Completed', '2024-11-02 16:20:00', '2024-11-02 16:25:00'),
                            (2, 'Daily Conversation', 24.99, 'Cancelled', '2024-11-03 11:45:00', NULL);
                    ";

                    seedCommand.CommandText = seedOrderData;
                    await seedCommand.ExecuteNonQueryAsync();

                    // Додати позиції замовлень
                    var seedOrderItemsData = @"
                        INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal) 
                        VALUES 
                            (1, 1, 1, 29.99, 29.99),
                            (2, 2, 1, 19.99, 19.99),
                            (3, 3, 1, 39.99, 39.99),
                            (4, 4, 1, 49.99, 49.99),
                            (5, 5, 1, 24.99, 24.99);
                    ";

                    seedCommand.CommandText = seedOrderItemsData;
                    await seedCommand.ExecuteNonQueryAsync();

                    // Додати платежі
                    var seedPaymentsData = @"
                        INSERT INTO Payments (OrderId, Amount, PaymentMethod, TransactionId, Status, PaymentDate) 
                        VALUES 
                            (1, 29.99, 'Card', 'TXN_001', 'Completed', '2024-10-01 10:05:00'),
                            (2, 19.99, 'PayPal', 'TXN_002', 'Completed', '2024-10-15 14:35:00'),
                            (3, 39.99, 'Card', 'TXN_003', 'Pending', '2024-11-01 09:15:00'),
                            (4, 49.99, 'Bank', 'TXN_004', 'Completed', '2024-11-02 16:25:00'),
                            (5, 24.99, 'Card', 'TXN_005', 'Failed', '2024-11-03 11:45:00');
                    ";

                    seedCommand.CommandText = seedPaymentsData;
                    await seedCommand.ExecuteNonQueryAsync();

                    Console.WriteLine("✅ Seed data added to Orders, OrderItems, and Payments.");
                }
            }
        }
    }
}