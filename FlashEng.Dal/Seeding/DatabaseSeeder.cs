using FlashEng.Dal.Context;
using FlashEng.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Seeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    Email = "admin@flasheng.com",
                    PasswordHash = "hashed_password_1", // В реальному проекті хешувати!
                    FullName = "Admin User",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "john@flasheng.com",
                    PasswordHash = "hashed_password_2",
                    FullName = "John Doe",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "jane@flasheng.com",
                    PasswordHash = "hashed_password_3",
                    FullName = "Jane Smith",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Seed UserSettings
            var userSettings = new List<UserSettings>
            {
                new UserSettings { UserId = users[0].UserId, Theme = "Dark", Language = "en", NotificationsEnabled = true },
                new UserSettings { UserId = users[1].UserId, Theme = "Light", Language = "en", NotificationsEnabled = true },
                new UserSettings { UserId = users[2].UserId, Theme = "Auto", Language = "uk", NotificationsEnabled = false }
            };

            await context.UserSettings.AddRangeAsync(userSettings);
            await context.SaveChangesAsync();

            // Seed Products
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Business English Course",
                    Price = 29.99m,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Travel Phrases Pack",
                    Price = 19.99m,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Advanced Grammar",
                    Price = 39.99m,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "IELTS Preparation",
                    Price = 49.99m,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Seed Flashcards
            var flashcards = new List<Flashcard>
            {
                new Flashcard
                {
                    UserId = users[0].UserId,
                    Category = "Food",
                    EnglishWord = "apple",
                    Translation = "яблуко",
                    Definition = "A round fruit with red or green skin",
                    ExampleSentence = "I eat an apple every day.",
                    Pronunciation = "/ˈæp.əl/",
                    Difficulty = "Easy",
                    IsPublic = false,
                    Price = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Flashcard
                {
                    UserId = users[0].UserId,
                    Category = "Food",
                    EnglishWord = "bread",
                    Translation = "хліб",
                    Definition = "A basic food made from flour and water",
                    ExampleSentence = "I buy fresh bread every morning.",
                    Pronunciation = "/bred/",
                    Difficulty = "Easy",
                    IsPublic = false,
                    Price = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Flashcard
                {
                    UserId = users[0].UserId,
                    Category = "Business English",
                    EnglishWord = "meeting",
                    Translation = "зустріч",
                    Definition = "A gathering of people for discussion",
                    ExampleSentence = "We have a meeting at 3 PM.",
                    Pronunciation = "/ˈmiː.tɪŋ/",
                    Difficulty = "Medium",
                    IsPublic = true,
                    Price = 29.99m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Flashcard
                {
                    UserId = users[1].UserId,
                    Category = "Travel Phrases",
                    EnglishWord = "airport",
                    Translation = "аеропорт",
                    Definition = "A place where planes take off and land",
                    ExampleSentence = "I need to be at the airport two hours early.",
                    Pronunciation = "/ˈeə.pɔːt/",
                    Difficulty = "Easy",
                    IsPublic = true,
                    Price = 19.99m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Flashcard
                {
                    UserId = users[1].UserId,
                    Category = "Business English",
                    EnglishWord = "presentation",
                    Translation = "презентація",
                    Definition = "A speech or talk in which something is shown",
                    ExampleSentence = "I need to prepare a presentation for tomorrow.",
                    Pronunciation = "/ˌprez.ənˈteɪ.ʃən/",
                    Difficulty = "Hard",
                    IsPublic = true,
                    Price = 29.99m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Flashcards.AddRangeAsync(flashcards);
            await context.SaveChangesAsync();

            // Seed Orders
            var orders = new List<Order>
            {
                new Order
                {
                    UserId = users[1].UserId,
                    TotalAmount = 49.98m,
                    Status = "Completed",
                    OrderDate = DateTime.UtcNow.AddDays(-10)
                },
                new Order
                {
                    UserId = users[2].UserId,
                    TotalAmount = 39.99m,
                    Status = "Pending",
                    OrderDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            // Seed OrderItems
            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderId = orders[0].OrderId,
                    ProductId = products[0].ProductId,
                    Quantity = 1,
                    UnitPrice = 29.99m,
                    LineTotal = 29.99m
                },
                new OrderItem
                {
                    OrderId = orders[0].OrderId,
                    ProductId = products[1].ProductId,
                    Quantity = 1,
                    UnitPrice = 19.99m,
                    LineTotal = 19.99m
                },
                new OrderItem
                {
                    OrderId = orders[1].OrderId,
                    ProductId = products[2].ProductId,
                    Quantity = 1,
                    UnitPrice = 39.99m,
                    LineTotal = 39.99m
                }
            };

            await context.OrderItems.AddRangeAsync(orderItems);
            await context.SaveChangesAsync();

            // Seed Payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    OrderId = orders[0].OrderId,
                    Amount = 49.98m,
                    PaymentMethod = "Card",
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }
    }
}
