using UserManagementService;

namespace FlashEngUserManagement
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - User Management Service");
            Console.WriteLine("  5 Tables + 6 Stored Procedures + All Relationships");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних, якщо її не існує
                await DatabaseConfig.EnsureDatabaseCreatedAsync();

                // Створюємо таблиці, якщо їх не існує
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new UserManagementRepository();

                Console.WriteLine("🎯 ДЕМОНСТРАЦІЯ ЗВ'ЯЗКІВ МІЖ ТАБЛИЦЯМИ");
                Console.WriteLine("=====================================");

                // 1. Показати всіх користувачів зі статистикою
                Console.WriteLine("\n--- 👥 ALL USERS WITH STATISTICS ---");
                var usersWithStats = await repository.GetUsersWithStatisticsAsync();
                foreach (var user in usersWithStats)
                {
                    Console.WriteLine($"ID: {user.UserId} | {user.FullName} | {user.Email}");
                    Console.WriteLine($"   Role: {user.Role} | Level: {user.EnglishLevel} | Active: {user.IsActive}");
                    Console.WriteLine($"   Theme: {user.Theme} | Language: {user.Language}");
                    Console.WriteLine($"   Subscriptions: {user.ActiveSubscriptions} | Skills: {user.SkillsCount}");
                    Console.WriteLine();
                }

                Console.WriteLine("\n🔗 ДЕМОНСТРАЦІЯ ЗВ'ЯЗКУ 1:1 (UserProfiles ↔ UserSettings)");
                Console.WriteLine("============================================================");

                // 2. Показати зв'язок 1:1 між UserProfiles та UserSettings
                foreach (var user in usersWithStats.Take(3))
                {
                    var settings = await repository.GetUserSettingsAsync(user.UserId);
                    Console.WriteLine($"👤 User: {user.FullName}");
                    if (settings != null)
                    {
                        Console.WriteLine($"   └── ⚙️ Settings: {settings.Theme} theme, {settings.Language} language, Notifications: {settings.NotificationsEnabled}");
                    }
                    else
                    {
                        Console.WriteLine($"   └── ❌ No settings found");
                    }
                }

                Console.WriteLine("\n🔗 ДЕМОНСТРАЦІЯ ЗВ'ЯЗКУ 1:N (UserProfiles → UserSubscriptions)");
                Console.WriteLine("================================================================");

                // 3. Показати зв'язок 1:N між UserProfiles та UserSubscriptions
                foreach (var user in usersWithStats.Take(3))
                {
                    var subscriptions = await repository.GetUserSubscriptionsAsync(user.UserId);
                    Console.WriteLine($"\n👤 User: {user.FullName}");
                    if (subscriptions.Any())
                    {
                        foreach (var sub in subscriptions)
                        {
                            Console.WriteLine($"   └── 💳 Subscription: {sub.PlanType} | ${sub.Price} | Active: {sub.IsActive} | {sub.StartDate:yyyy-MM-dd}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   └── ❌ No subscriptions");
                    }
                }

                Console.WriteLine("\n🔗 ДЕМОНСТРАЦІЯ ЗВ'ЯЗКУ M:N (UserProfiles ↔ Skills через UserSkillLevels)");
                Console.WriteLine("==========================================================================");

                // 4. Показати зв'язок M:N між користувачами та навичками
                var allSkills = await repository.GetAllSkillsAsync();
                Console.WriteLine($"📚 Total Skills Available: {allSkills.Count}");

                foreach (var user in usersWithStats.Take(3))
                {
                    var userSkills = await repository.GetUserSkillsAsync(user.UserId);
                    Console.WriteLine($"\n👤 User: {user.FullName}");
                    if (userSkills.Any())
                    {
                        foreach (var skill in userSkills)
                        {
                            Console.WriteLine($"   └── 🎯 {skill.SkillName} ({skill.Category}) - Level: {skill.Level} ({skill.Progress ?? 0}%)");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   └── ❌ No skills assigned");
                    }
                }

                Console.WriteLine("\n🎯 ДЕМОНСТРАЦІЯ ЗБЕРЕЖУВАНИХ ПРОЦЕДУР");
                Console.WriteLine("=====================================");

                // 5. Процедура: Створення користувача з налаштуваннями
                Console.WriteLine("\n--- 🏗️ ПРОЦЕДУРА: CreateUserWithSettings ---");
                try
                {
                    string testEmail = $"procedure_user_{DateTime.Now:yyyyMMddHHmmss}@flasheng.com";
                    int newUserId = await repository.CreateUserWithSettingsAsync(
                        testEmail,
                        "hashed_password_proc",
                        "Procedure Test User",
                        "Dark",
                        "uk"
                    );
                    Console.WriteLine($"✅ User created via procedure: ID = {newUserId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Procedure failed: {ex.Message}");
                }

                // 6. Процедура: Отримання повного профілю
                Console.WriteLine("\n--- 📋 ПРОЦЕДУРА: GetUserFullProfile ---");
                try
                {
                    var (profile, settings, subscriptions, skills) = await repository.GetUserFullProfileAsync(1);
                    if (profile != null)
                    {
                        Console.WriteLine($"👤 Profile: {profile.FullName} ({profile.Email})");
                        Console.WriteLine($"⚙️ Settings: {settings?.Theme ?? "None"} theme");
                        Console.WriteLine($"💳 Subscriptions: {subscriptions.Count} total");
                        Console.WriteLine($"🎯 Skills: {skills.Count} assigned");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ GetUserFullProfile failed: {ex.Message}");
                }

                // 7. Процедура: Оновлення підписки
                Console.WriteLine("\n--- 💳 ПРОЦЕДУРА: UpdateUserSubscription ---");
                try
                {
                    var message = await repository.UpdateUserSubscriptionAsync(2, "Pro", 6);
                    Console.WriteLine($"✅ {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ UpdateUserSubscription failed: {ex.Message}");
                }

                // 8. Процедура: Статистика користувачів
                Console.WriteLine("\n--- 📊 ПРОЦЕДУРА: GetUsersStatistics ---");
                try
                {
                    var stats = await repository.GetUsersStatisticsAsync();
                    foreach (var stat in stats)
                    {
                        Console.WriteLine($"📈 {stat.StatType} | {stat.Category}: {stat.TotalCount} total, {stat.ActiveCount} active ({stat.ActivePercentage}%)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ GetUsersStatistics failed: {ex.Message}");
                }

                // 9. Процедура: Масове оновлення навичок
                Console.WriteLine("\n--- 🎯 ПРОЦЕДУРА: BulkUpdateSkillLevels ---");
                try
                {
                    // Формат: skillId:level:progress,skillId:level:progress
                    string skillUpdates = "1:Advanced:85,2:Intermediate:70,5:Expert:95";
                    int updatedCount = await repository.BulkUpdateSkillLevelsAsync(2, skillUpdates);
                    Console.WriteLine($"✅ Bulk update completed: {updatedCount} skills updated");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ BulkUpdateSkillLevels failed: {ex.Message}");
                }

                Console.WriteLine("\n🎯 ДОДАТКОВІ CRUD ОПЕРАЦІЇ");
                Console.WriteLine("==========================");

                // 10. Створити нову навичку
                Console.WriteLine("\n--- ➕ СТВОРЕННЯ НОВОЇ НАВИЧКИ ---");
                try
                {
                    int newSkillId = await repository.CreateSkillAsync("AI Prompting", "Technology");
                    Console.WriteLine($"✅ New skill created: ID = {newSkillId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Skill creation failed: {ex.Message}");
                }

                // 11. Додати навичку користувачу
                Console.WriteLine("\n--- ➕ ДОДАВАННЯ НАВИЧКИ КОРИСТУВАЧУ ---");
                try
                {
                    bool added = await repository.AddUserSkillAsync(3, 1, "Intermediate");
                    Console.WriteLine($"✅ Skill added to user: {added}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Adding skill failed: {ex.Message}");
                }

                // 12. Статистика навичок по категоріях
                Console.WriteLine("\n--- 📊 СТАТИСТИКА НАВИЧОК ПО КАТЕГОРІЯХ ---");
                var skillStats = await repository.GetSkillCategoryStatisticsAsync();
                foreach (var stat in skillStats)
                {
                    Console.WriteLine($"📚 {stat.Category}: {stat.TotalSkills} skills | {stat.UsersWithSkills} users | Avg level: {stat.AverageLevel:F1}");
                }

                // 13. Пошук користувачів з фільтрами
                Console.WriteLine("\n--- 🔍 ПОШУК КОРИСТУВАЧІВ ---");
                var searchResults = await repository.SearchUsersAsync(role: "User", isActive: true);
                Console.WriteLine($"Found {searchResults.Count} active users with 'User' role:");
                foreach (var user in searchResults.Take(3))
                {
                    Console.WriteLine($"   - {user.FullName} ({user.Email}) - Level: {user.EnglishLevel}");
                }

                // 14. Процедура: Каскадне деактивування
                Console.WriteLine("\n--- ❌ ПРОЦЕДУРА: DeactivateUserCascade ---");
                try
                {
                    // Знайти користувача для деактивації
                    var inactiveUsers = await repository.SearchUsersAsync(isActive: false);
                    if (inactiveUsers.Any())
                    {
                        var userToDeactivate = inactiveUsers.First();
                        var message = await repository.DeactivateUserCascadeAsync(userToDeactivate.UserId, "Testing cascade deactivation");
                        Console.WriteLine($"📝 {message}");
                    }
                    else
                    {
                        Console.WriteLine("No inactive users found for testing");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ DeactivateUserCascade failed: {ex.Message}");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  ✅ УСПІШНО ПРОДЕМОНСТРОВАНО:");
                Console.WriteLine("  📊 5 таблиць з Foreign Key зв'язками");
                Console.WriteLine("  🔗 Зв'язки: 1:1, 1:N, M:N");
                Console.WriteLine("  ⚙️ 6 збережуваних процедур");
                Console.WriteLine("  🛠️ Повний CRUD для всіх сутностей");
                Console.WriteLine("  📈 Комплексна бізнес-логіка");
                Console.WriteLine("  🎯 Bounded Context: User Management");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ КРИТИЧНА ПОМИЛКА: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}