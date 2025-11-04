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
        /// Створити базу даних, якщо її не існує
        /// </summary>
        public static async Task EnsureDatabaseCreatedAsync()
        {
            using var connection = new MySqlConnection(ServerConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_user_management;";

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Database 'flasheng_user_management' ensured.");
        }

        /// <summary>
        /// Створити всі таблиці з правильними зв'язками
        /// </summary>
        public static async Task EnsureTablesCreatedAsync()
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // 1. Таблиця UserProfiles (основна)
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
                    INDEX idx_active (IsActive),
                    INDEX idx_english_level (EnglishLevel)
                );";

            // 2. Таблиця UserSettings (1:1 з UserProfiles)
            var createUserSettingsTable = @"
                CREATE TABLE IF NOT EXISTS UserSettings (
                    SettingsId INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT UNIQUE NOT NULL,
                    Theme VARCHAR(20) DEFAULT 'Light',
                    Language VARCHAR(5) DEFAULT 'en',
                    NotificationsEnabled BOOLEAN DEFAULT TRUE,
                    EmailNotifications BOOLEAN DEFAULT TRUE,
                    PushNotifications BOOLEAN DEFAULT TRUE,
                    TimeZone VARCHAR(50) DEFAULT 'UTC',
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES UserProfiles(UserId) ON DELETE CASCADE,
                    INDEX idx_theme (Theme),
                    INDEX idx_language (Language)
                );";

            // 3. Таблиця UserSubscriptions (1:N з UserProfiles)
            var createUserSubscriptionsTable = @"
                CREATE TABLE IF NOT EXISTS UserSubscriptions (
                    SubscriptionId INT AUTO_INCREMENT PRIMARY KEY,
                    UserId INT NOT NULL,
                    PlanType VARCHAR(50) NOT NULL,
                    Price DECIMAL(10,2) DEFAULT 0.00,
                    StartDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    EndDate TIMESTAMP NULL,
                    IsActive BOOLEAN DEFAULT TRUE,
                    AutoRenew BOOLEAN DEFAULT FALSE,
                    PaymentMethod VARCHAR(50) DEFAULT '',
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES UserProfiles(UserId) ON DELETE CASCADE,
                    INDEX idx_user_subscriptions (UserId),
                    INDEX idx_plan_type (PlanType),
                    INDEX idx_active (IsActive),
                    INDEX idx_dates (StartDate, EndDate),
                    CONSTRAINT chk_price CHECK (Price >= 0),
                    CONSTRAINT chk_dates CHECK (EndDate IS NULL OR EndDate > StartDate)
                );";

            // 4. Таблиця Skills (довідник для M:N)
            var createSkillsTable = @"
                CREATE TABLE IF NOT EXISTS Skills (
                    SkillId INT AUTO_INCREMENT PRIMARY KEY,
                    SkillName VARCHAR(100) UNIQUE NOT NULL,
                    Category VARCHAR(50) NOT NULL,
                    Description TEXT,
                    DifficultyLevel VARCHAR(20) DEFAULT 'Beginner',
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_category (Category),
                    INDEX idx_difficulty (DifficultyLevel),
                    INDEX idx_active (IsActive)
                );";

            // 5. Таблиця UserSkillLevels (M:N між UserProfiles та Skills)
            var createUserSkillLevelsTable = @"
                CREATE TABLE IF NOT EXISTS UserSkillLevels (
                    UserId INT NOT NULL,
                    SkillId INT NOT NULL,
                    Level VARCHAR(20) DEFAULT 'Beginner',
                    Progress INT DEFAULT 0,
                    LastAssessed TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    NextAssessment TIMESTAMP NULL,
                    Notes TEXT,
                    PRIMARY KEY (UserId, SkillId),
                    FOREIGN KEY (UserId) REFERENCES UserProfiles(UserId) ON DELETE CASCADE,
                    FOREIGN KEY (SkillId) REFERENCES Skills(SkillId) ON DELETE CASCADE,
                    INDEX idx_level (Level),
                    INDEX idx_progress (Progress),
                    INDEX idx_last_assessed (LastAssessed),
                    CONSTRAINT chk_progress CHECK (Progress >= 0 AND Progress <= 100)
                );";

            var command = connection.CreateCommand();

            // Створюємо таблиці в правильному порядку (FK залежності)
            command.CommandText = createUserProfilesTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserProfiles' ensured.");

            command.CommandText = createUserSettingsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserSettings' ensured (1:1 relationship).");

            command.CommandText = createUserSubscriptionsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserSubscriptions' ensured (1:N relationship).");

            command.CommandText = createSkillsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'Skills' ensured.");

            command.CommandText = createUserSkillLevelsTable;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Table 'UserSkillLevels' ensured (M:N relationship).");

            // Створити збережувані процедури
            await CreateStoredProceduresAsync(connection);

            // Додати початкові дані
            await SeedDataAsync(connection);
        }

        /// <summary>
        /// Створити збережувані процедури (6 штук згідно вимог)
        /// </summary>
        private static async Task CreateStoredProceduresAsync(MySqlConnection connection)
        {
            // 1. Процедура створення користувача з налаштуваннями (демонструє 1:1 зв'язок)
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
                    DECLARE EXIT HANDLER FOR SQLEXCEPTION
                    BEGIN
                        ROLLBACK;
                        RESIGNAL;
                    END;
                    
                    START TRANSACTION;
                    
                    -- Створити користувача
                    INSERT INTO UserProfiles (Email, PasswordHash, FullName, IsActive, CreatedAt, UpdatedAt)
                    VALUES (p_Email, p_Password, p_FullName, TRUE, NOW(), NOW());
                    
                    SET p_UserId = LAST_INSERT_ID();
                    
                    -- Створити налаштування (1:1 зв'язок)
                    INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled, CreatedAt, UpdatedAt)
                    VALUES (p_UserId, p_Theme, p_Language, TRUE, NOW(), NOW());
                    
                    COMMIT;
                END;";

            // 2. Процедура отримання повного профілю (демонструє всі зв'язки)
            var getUserFullProfileProcedure = @"
                DROP PROCEDURE IF EXISTS GetUserFullProfile;
                CREATE PROCEDURE GetUserFullProfile(IN p_UserId INT)
                BEGIN
                    -- Основний профіль з налаштуваннями (1:1 JOIN)
                    SELECT 
                        u.*,
                        s.Theme, s.Language, s.NotificationsEnabled as SettingsNotifications,
                        s.EmailNotifications, s.PushNotifications, s.TimeZone
                    FROM UserProfiles u
                    LEFT JOIN UserSettings s ON u.UserId = s.UserId
                    WHERE u.UserId = p_UserId;
                    
                    -- Підписки користувача (1:N)
                    SELECT * FROM UserSubscriptions 
                    WHERE UserId = p_UserId 
                    ORDER BY StartDate DESC;
                    
                    -- Навички користувача (M:N через JOIN)
                    SELECT 
                        s.SkillId, s.SkillName, s.Category, s.Description,
                        usk.Level, usk.Progress, usk.LastAssessed, usk.NextAssessment, usk.Notes
                    FROM UserSkillLevels usk
                    JOIN Skills s ON usk.SkillId = s.SkillId
                    WHERE usk.UserId = p_UserId
                    ORDER BY s.Category, s.SkillName;
                END;";

            // 3. Процедура оновлення підписки (демонструє 1:N операції)
            var updateUserSubscriptionProcedure = @"
                DROP PROCEDURE IF EXISTS UpdateUserSubscription;
                CREATE PROCEDURE UpdateUserSubscription(
                    IN p_UserId INT,
                    IN p_PlanType VARCHAR(50),
                    IN p_DurationMonths INT
                )
                BEGIN
                    DECLARE v_Price DECIMAL(10,2);
                    DECLARE v_EndDate TIMESTAMP;
                    
                    -- Розрахувати ціну та дату закінчення
                    SET v_Price = CASE 
                        WHEN p_PlanType = 'Premium' THEN 9.99 * p_DurationMonths
                        WHEN p_PlanType = 'Pro' THEN 19.99 * p_DurationMonths
                        WHEN p_PlanType = 'Enterprise' THEN 49.99 * p_DurationMonths
                        ELSE 0
                    END;
                    
                    SET v_EndDate = DATE_ADD(NOW(), INTERVAL p_DurationMonths MONTH);
                    
                    -- Деактивувати попередні підписки
                    UPDATE UserSubscriptions 
                    SET IsActive = FALSE 
                    WHERE UserId = p_UserId AND IsActive = TRUE;
                    
                    -- Створити нову підписку
                    INSERT INTO UserSubscriptions (UserId, PlanType, Price, StartDate, EndDate, IsActive)
                    VALUES (p_UserId, p_PlanType, v_Price, NOW(), v_EndDate, TRUE);
                    
                    SELECT CONCAT('Subscription updated: ', p_PlanType, ' for ', p_DurationMonths, ' months. Price: $', v_Price) as Message;
                END;";

            // 4. Процедура каскадного деактивації користувача
            var deactivateUserCascadeProcedure = @"
                DROP PROCEDURE IF EXISTS DeactivateUserCascade;
                CREATE PROCEDURE DeactivateUserCascade(
                    IN p_UserId INT,
                    IN p_Reason VARCHAR(255)
                )
                BEGIN
                    DECLARE EXIT HANDLER FOR SQLEXCEPTION
                    BEGIN
                        ROLLBACK;
                        RESIGNAL;
                    END;
                    
                    START TRANSACTION;
                    
                    -- Деактивувати користувача
                    UPDATE UserProfiles SET IsActive = FALSE, UpdatedAt = NOW() WHERE UserId = p_UserId;
                    
                    -- Деактивувати всі підписки
                    UPDATE UserSubscriptions SET IsActive = FALSE WHERE UserId = p_UserId;
                    
                    -- Очистити персональні налаштування (опціонально)
                    UPDATE UserSettings 
                    SET NotificationsEnabled = FALSE, EmailNotifications = FALSE, PushNotifications = FALSE
                    WHERE UserId = p_UserId;
                    
                    COMMIT;
                    
                    SELECT CONCAT('User ', p_UserId, ' deactivated. Reason: ', p_Reason) as Message;
                END;";

            // 5. Процедура статистики користувачів
            var getUsersStatisticsProcedure = @"
                DROP PROCEDURE IF EXISTS GetUsersStatistics;
                CREATE PROCEDURE GetUsersStatistics()
                BEGIN
                    SELECT 
                        'Users by Role' as StatType,
                        Role as Category,
                        COUNT(*) as TotalCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveCount,
                        ROUND(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) as ActivePercentage
                    FROM UserProfiles 
                    GROUP BY Role
                    
                    UNION ALL
                    
                    SELECT 
                        'Subscriptions by Plan' as StatType,
                        PlanType as Category,
                        COUNT(*) as TotalCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveCount,
                        ROUND(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) as ActivePercentage
                    FROM UserSubscriptions 
                    GROUP BY PlanType
                    
                    UNION ALL
                    
                    SELECT 
                        'Skills by Category' as StatType,
                        s.Category as Category,
                        COUNT(DISTINCT s.SkillId) as TotalCount,
                        COUNT(DISTINCT usk.UserId) as ActiveCount,
                        ROUND(COUNT(DISTINCT usk.UserId) * 100.0 / (SELECT COUNT(*) FROM UserProfiles WHERE IsActive = 1), 2) as ActivePercentage
                    FROM Skills s
                    LEFT JOIN UserSkillLevels usk ON s.SkillId = usk.SkillId
                    WHERE s.IsActive = 1
                    GROUP BY s.Category
                    
                    ORDER BY StatType, Category;
                END;";

            // 6. Процедура масового оновлення рівнів навичок
            var bulkUpdateSkillLevelsProcedure = @"
                DROP PROCEDURE IF EXISTS BulkUpdateSkillLevels;
                CREATE PROCEDURE BulkUpdateSkillLevels(
                    IN p_UserId INT,
                    IN p_SkillUpdates TEXT  -- Формат: 'skillId1:level1:progress1,skillId2:level2:progress2'
                )
                BEGIN
                    DECLARE v_SkillId INT;
                    DECLARE v_Level VARCHAR(20);
                    DECLARE v_Progress INT;
                    DECLARE v_UpdateCount INT DEFAULT 0;
                    DECLARE v_Item TEXT;
                    DECLARE v_Position INT DEFAULT 1;
                    DECLARE v_CommaPos INT;
                    
                    DECLARE EXIT HANDLER FOR SQLEXCEPTION
                    BEGIN
                        ROLLBACK;
                        RESIGNAL;
                    END;
                    
                    START TRANSACTION;
                    
                    -- Парсинг рядка та оновлення навичок
                    WHILE v_Position <= CHAR_LENGTH(p_SkillUpdates) DO
                        SET v_CommaPos = LOCATE(',', p_SkillUpdates, v_Position);
                        
                        IF v_CommaPos = 0 THEN
                            SET v_Item = SUBSTRING(p_SkillUpdates, v_Position);
                            SET v_Position = CHAR_LENGTH(p_SkillUpdates) + 1;
                        ELSE
                            SET v_Item = SUBSTRING(p_SkillUpdates, v_Position, v_CommaPos - v_Position);
                            SET v_Position = v_CommaPos + 1;
                        END IF;
                        
                        -- Парсинг окремого елементу (skillId:level:progress)
                        SET v_SkillId = CAST(SUBSTRING_INDEX(v_Item, ':', 1) AS UNSIGNED);
                        SET v_Level = SUBSTRING_INDEX(SUBSTRING_INDEX(v_Item, ':', 2), ':', -1);
                        SET v_Progress = CAST(SUBSTRING_INDEX(v_Item, ':', -1) AS UNSIGNED);
                        
                        -- Оновлення або вставка навички
                        INSERT INTO UserSkillLevels (UserId, SkillId, Level, Progress, LastAssessed)
                        VALUES (p_UserId, v_SkillId, v_Level, v_Progress, NOW())
                        ON DUPLICATE KEY UPDATE 
                            Level = v_Level, 
                            Progress = v_Progress, 
                            LastAssessed = NOW();
                            
                        SET v_UpdateCount = v_UpdateCount + 1;
                    END WHILE;
                    
                    COMMIT;
                    
                    SELECT v_UpdateCount as UpdatedCount;
                END;";

            var command = connection.CreateCommand();

            command.CommandText = createUserWithSettingsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'CreateUserWithSettings' created.");

            command.CommandText = getUserFullProfileProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'GetUserFullProfile' created.");

            command.CommandText = updateUserSubscriptionProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'UpdateUserSubscription' created.");

            command.CommandText = deactivateUserCascadeProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'DeactivateUserCascade' created.");

            command.CommandText = getUsersStatisticsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'GetUsersStatistics' created.");

            command.CommandText = bulkUpdateSkillLevelsProcedure;
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Stored procedure 'BulkUpdateSkillLevels' created.");
        }

        /// <summary>
        /// Додати початкові дані для демонстрації всіх зв'язків
        /// </summary>
        private static async Task SeedDataAsync(MySqlConnection connection)
        {
            // Перевірити, чи є дані
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM UserProfiles;";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                // 1. Додати користувачів
                var seedUserData = @"
                    INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, EnglishLevel, PreferredAIModel, DailyGoal, NotificationsEnabled, IsActive) 
                    VALUES 
                        ('admin@flasheng.com', 'hashed_admin_password', 'Admin User', 'Admin', 'C2', 'GPT-4', 30, TRUE, TRUE),
                        ('john.doe@flasheng.com', 'hashed_user1_password', 'John Doe', 'User', 'B2', 'GPT-3.5', 15, TRUE, TRUE),
                        ('jane.smith@flasheng.com', 'hashed_user2_password', 'Jane Smith', 'Premium', 'B1', 'GPT-4', 25, TRUE, TRUE),
                        ('mike.wilson@flasheng.com', 'hashed_user3_password', 'Mike Wilson', 'User', 'A2', 'GPT-3.5', 10, TRUE, TRUE),
                        ('sara.brown@flasheng.com', 'hashed_user4_password', 'Sara Brown', 'User', 'C1', 'GPT-4', 20, FALSE, FALSE);
                ";

                var seedCommand = connection.CreateCommand();
                seedCommand.CommandText = seedUserData;
                await seedCommand.ExecuteNonQueryAsync();

                // 2. Додати налаштування користувачів (1:1 зв'язок)
                var seedUserSettingsData = @"
                    INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled, EmailNotifications, PushNotifications, TimeZone) 
                    VALUES 
                        (1, 'Dark', 'en', TRUE, TRUE, TRUE, 'America/New_York'),
                        (2, 'Light', 'en', TRUE, TRUE, FALSE, 'Europe/London'),
                        (3, 'Auto', 'uk', TRUE, FALSE, TRUE, 'Europe/Kiev'),
                        (4, 'Light', 'es', FALSE, FALSE, FALSE, 'Europe/Madrid'),
                        (5, 'Dark', 'en', FALSE, FALSE, FALSE, 'UTC');
                ";

                seedCommand.CommandText = seedUserSettingsData;
                await seedCommand.ExecuteNonQueryAsync();

                // 3. Додати підписки (1:N зв'язок)
                var seedSubscriptionsData = @"
                    INSERT INTO UserSubscriptions (UserId, PlanType, Price, StartDate, EndDate, IsActive, AutoRenew, PaymentMethod) 
                    VALUES 
                        (1, 'Enterprise', 599.88, '2024-01-01 00:00:00', '2024-12-31 23:59:59', TRUE, TRUE, 'Corporate'),
                        (2, 'Premium', 119.88, '2024-10-01 00:00:00', '2024-12-31 23:59:59', TRUE, FALSE, 'Card'),
                        (2, 'Free', 0.00, '2024-08-01 00:00:00', '2024-09-30 23:59:59', FALSE, FALSE, 'None'),
                        (3, 'Pro', 239.88, '2024-09-15 00:00:00', '2025-03-15 23:59:59', TRUE, TRUE, 'PayPal'),
                        (4, 'Free', 0.00, '2024-11-01 00:00:00', NULL, TRUE, FALSE, 'None'),
                        (5, 'Premium', 59.88, '2024-05-01 00:00:00', '2024-10-31 23:59:59', FALSE, FALSE, 'Card');
                ";

                seedCommand.CommandText = seedSubscriptionsData;
                await seedCommand.ExecuteNonQueryAsync();

                // 4. Додати навички (довідник для M:N)
                var seedSkillsData = @"
                    INSERT INTO Skills (SkillName, Category, Description, DifficultyLevel, IsActive) 
                    VALUES 
                        ('Present Simple', 'Grammar', 'Basic present tense usage', 'Beginner', TRUE),
                        ('Past Simple', 'Grammar', 'Basic past tense usage', 'Beginner', TRUE),
                        ('Present Perfect', 'Grammar', 'Present perfect tense usage', 'Intermediate', TRUE),
                        ('Conditionals', 'Grammar', 'If-clauses and conditional sentences', 'Advanced', TRUE),
                        
                        ('Business Vocabulary', 'Vocabulary', 'Professional business terms', 'Intermediate', TRUE),
                        ('Travel Vocabulary', 'Vocabulary', 'Travel-related words and phrases', 'Beginner', TRUE),
                        ('Academic Vocabulary', 'Vocabulary', 'Academic and scientific terms', 'Advanced', TRUE),
                        
                        ('Job Interview', 'Speaking', 'Interview conversation skills', 'Intermediate', TRUE),
                        ('Small Talk', 'Speaking', 'Casual conversation skills', 'Beginner', TRUE),
                        ('Presentations', 'Speaking', 'Public speaking and presentations', 'Advanced', TRUE),
                        
                        ('News Articles', 'Reading', 'Reading news and articles', 'Intermediate', TRUE),
                        ('Academic Texts', 'Reading', 'Reading academic papers', 'Advanced', TRUE),
                        
                        ('Business Writing', 'Writing', 'Professional email and reports', 'Intermediate', TRUE),
                        ('Creative Writing', 'Writing', 'Creative and descriptive writing', 'Advanced', TRUE),
                        
                        ('Phone Conversations', 'Listening', 'Understanding phone calls', 'Intermediate', TRUE),
                        ('Movies and TV', 'Listening', 'Understanding entertainment media', 'Advanced', TRUE);
                ";

                seedCommand.CommandText = seedSkillsData;
                await seedCommand.ExecuteNonQueryAsync();

                // 5. Додати рівні навичок користувачів (M:N зв'язок)
                var seedUserSkillsData = @"
                    INSERT INTO UserSkillLevels (UserId, SkillId, Level, Progress, LastAssessed, Notes) 
                    VALUES 
                        -- Admin (ID=1) - експерт у всьому
                        (1, 1, 'Expert', 100, '2024-11-01 10:00:00', 'Perfect knowledge'),
                        (1, 2, 'Expert', 100, '2024-11-01 10:00:00', 'Perfect knowledge'),
                        (1, 5, 'Expert', 95, '2024-11-01 10:00:00', 'Excellent business vocabulary'),
                        (1, 8, 'Expert', 90, '2024-11-01 10:00:00', 'Great interview skills'),
                        
                        -- John (ID=2) - intermediate рівень
                        (2, 1, 'Advanced', 85, '2024-10-15 14:30:00', 'Good understanding'),
                        (2, 2, 'Advanced', 80, '2024-10-15 14:30:00', 'Solid knowledge'),
                        (2, 3, 'Intermediate', 60, '2024-10-15 14:30:00', 'Still learning'),
                        (2, 5, 'Intermediate', 70, '2024-10-15 14:30:00', 'Business context'),
                        (2, 9, 'Advanced', 75, '2024-10-15 14:30:00', 'Confident in small talk'),
                        
                        -- Jane (ID=3) - balanced skills
                        (3, 1, 'Intermediate', 65, '2024-09-20 09:15:00', 'Good basics'),
                        (3, 6, 'Beginner', 40, '2024-09-20 09:15:00', 'Just started'),
                        (3, 9, 'Intermediate', 55, '2024-09-20 09:15:00', 'Improving'),
                        (3, 11, 'Intermediate', 70, '2024-09-20 09:15:00', 'Reads news regularly'),
                        
                        -- Mike (ID=4) - beginner
                        (4, 1, 'Beginner', 30, '2024-11-01 16:00:00', 'Just started'),
                        (4, 6, 'Beginner', 25, '2024-11-01 16:00:00', 'Learning travel words'),
                        (4, 9, 'Beginner', 20, '2024-11-01 16:00:00', 'Shy but trying'),
                        
                        -- Sara (ID=5) - advanced but inactive
                        (5, 3, 'Advanced', 80, '2024-05-15 12:00:00', 'Was progressing well'),
                        (5, 4, 'Intermediate', 60, '2024-05-15 12:00:00', 'Complex grammar'),
                        (5, 13, 'Advanced', 85, '2024-05-15 12:00:00', 'Excellent writing skills');
                ";

                seedCommand.CommandText = seedUserSkillsData;
                await seedCommand.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Seed data added successfully:");
                Console.WriteLine("   - 5 users (UserProfiles)");
                Console.WriteLine("   - 5 user settings (1:1 relationship)");
                Console.WriteLine("   - 6 subscriptions (1:N relationship)");
                Console.WriteLine("   - 16 skills (reference table)");
                Console.WriteLine("   - 19 user skill levels (M:N relationship)");
            }
        }
    }
}