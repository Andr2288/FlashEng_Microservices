using FlashEng.Dal.Context;
using FlashEng.Domain.models;
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
                    Email = "chornookyiandry228@gmail.com",
                    PasswordHash = "1234567890",
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
    // FOOD категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Food",
        EnglishWord = "apple",
        Translation = "яблуко",
        Definition = "A round fruit with red or green skin",
        ExampleSentence = "I eat an apple every day.",
        Pronunciation = "/ˈæp.əl/",
        Difficulty = "Beginner",
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
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Food",
        EnglishWord = "pizza",
        Translation = "піца",
        Definition = "An Italian dish with bread, tomato sauce and cheese",
        ExampleSentence = "Let's order pizza for dinner tonight.",
        Pronunciation = "/ˈpiːt.sə/",
        Difficulty = "Beginner",
        IsPublic = true,
        Price = 2.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Food",
        EnglishWord = "restaurant",
        Translation = "ресторан",
        Definition = "A place where you can buy and eat meals",
        ExampleSentence = "We went to a fancy restaurant for our anniversary.",
        Pronunciation = "/ˈres.tər.ɑːnt/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 1.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // ANIMALS категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Animals",
        EnglishWord = "cat",
        Translation = "кіт",
        Definition = "A small furry animal often kept as a pet",
        ExampleSentence = "My cat loves to sleep in the sun.",
        Pronunciation = "/kæt/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Animals",
        EnglishWord = "elephant",
        Translation = "слон",
        Definition = "A very large gray animal with a long nose called a trunk",
        ExampleSentence = "The elephant sprayed water with its trunk.",
        Pronunciation = "/ˈel.ɪ.fənt/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 1.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Animals",
        EnglishWord = "butterfly",
        Translation = "метелик",
        Definition = "A flying insect with large colorful wings",
        ExampleSentence = "A beautiful butterfly landed on the flower.",
        Pronunciation = "/ˈbʌt.ɚ.flaɪ/",
        Difficulty = "Intermediate",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // TRAVEL категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Travel",
        EnglishWord = "airport",
        Translation = "аеропорт",
        Definition = "A place where airplanes take off and land",
        ExampleSentence = "Please arrive at the airport two hours early.",
        Pronunciation = "/ˈer.pɔːrt/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Travel",
        EnglishWord = "passport",
        Translation = "паспорт",
        Definition = "An official document that allows you to travel abroad",
        ExampleSentence = "Don't forget to bring your passport to the airport.",
        Pronunciation = "/ˈpæs.pɔːrt/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 3.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Travel",
        EnglishWord = "luggage",
        Translation = "багаж",
        Definition = "Bags and suitcases that you take when traveling",
        ExampleSentence = "My luggage was lost during the flight connection.",
        Pronunciation = "/ˈlʌɡ.ɪdʒ/",
        Difficulty = "Intermediate",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // TECHNOLOGY категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Technology",
        EnglishWord = "computer",
        Translation = "комп'ютер",
        Definition = "An electronic machine that can store and process data",
        ExampleSentence = "I use my computer for work every day.",
        Pronunciation = "/kəmˈpjuː.tɚ/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Technology",
        EnglishWord = "smartphone",
        Translation = "смартфон",
        Definition = "A mobile phone that can connect to the internet",
        ExampleSentence = "My smartphone battery lasts all day.",
        Pronunciation = "/ˈsmɑːrt.foʊn/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 4.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Technology",
        EnglishWord = "artificial intelligence",
        Translation = "штучний інтелект",
        Definition = "Computer systems that can perform tasks normally requiring human intelligence",
        ExampleSentence = "Artificial intelligence is changing many industries.",
        Pronunciation = "/ˌɑːr.tə.fɪʃ.əl ɪnˈtel.ə.dʒəns/",
        Difficulty = "Advanced",
        IsPublic = true,
        Price = 9.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // BUSINESS категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Business",
        EnglishWord = "meeting",
        Translation = "зустріч",
        Definition = "A gathering of people to discuss business matters",
        ExampleSentence = "We have a team meeting every Monday morning.",
        Pronunciation = "/ˈmiː.tɪŋ/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Business",
        EnglishWord = "presentation",
        Translation = "презентація",
        Definition = "A formal talk giving information about a particular subject",
        ExampleSentence = "She gave an excellent presentation about our quarterly results.",
        Pronunciation = "/ˌprez.ənˈteɪ.ʃən/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 5.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Business",
        EnglishWord = "entrepreneurship",
        Translation = "підприємництво",
        Definition = "The activity of setting up businesses and taking financial risks",
        ExampleSentence = "Entrepreneurship requires creativity and perseverance.",
        Pronunciation = "/ˌɑːn.trə.prəˈnɝː.ʃɪp/",
        Difficulty = "Advanced",
        IsPublic = true,
        Price = 12.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // HEALTH категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Health",
        EnglishWord = "exercise",
        Translation = "вправи",
        Definition = "Physical activity to improve health and fitness",
        ExampleSentence = "I do exercise three times a week.",
        Pronunciation = "/ˈek.sɚ.saɪz/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Health",
        EnglishWord = "nutrition",
        Translation = "харчування",
        Definition = "The process of eating the right kind of food for good health",
        ExampleSentence = "Good nutrition is essential for a healthy lifestyle.",
        Pronunciation = "/nuˈtrɪʃ.ən/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 4.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Health",
        EnglishWord = "metabolism",
        Translation = "метаболізм",
        Definition = "The chemical processes in the body that convert food to energy",
        ExampleSentence = "Regular exercise can boost your metabolism.",
        Pronunciation = "/məˈtæb.ə.lɪ.zəm/",
        Difficulty = "Advanced",
        IsPublic = true,
        Price = 8.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // EDUCATION категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Education",
        EnglishWord = "student",
        Translation = "студент",
        Definition = "A person who is studying at a school or university",
        ExampleSentence = "She is a student at the local university.",
        Pronunciation = "/ˈstuː.dənt/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Education",
        EnglishWord = "curriculum",
        Translation = "навчальна програма",
        Definition = "The subjects that are taught in a school or college",
        ExampleSentence = "The new curriculum includes more practical skills.",
        Pronunciation = "/kəˈrɪk.jə.ləm/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 6.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[2].UserId,
        Category = "Education",
        EnglishWord = "pedagogy",
        Translation = "педагогіка",
        Definition = "The method and practice of teaching",
        ExampleSentence = "Modern pedagogy emphasizes interactive learning methods.",
        Pronunciation = "/ˈped.əˌɡɑː.dʒi/",
        Difficulty = "Advanced",
        IsPublic = true,
        Price = 11.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // SPORTS категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Sports",
        EnglishWord = "football",
        Translation = "футбол",
        Definition = "A game played between two teams using a round ball",
        ExampleSentence = "We play football every Saturday in the park.",
        Pronunciation = "/ˈfʊt.bɔːl/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Sports",
        EnglishWord = "championship",
        Translation = "чемпіонат",
        Definition = "A competition to find the best team or player in a sport",
        ExampleSentence = "Our team won the regional championship this year.",
        Pronunciation = "/ˈtʃæm.pi.ən.ʃɪp/",
        Difficulty = "Intermediate",
        IsPublic = true,
        Price = 3.99m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },

    // NATURE категорія
    new Flashcard
    {
        UserId = users[0].UserId,
        Category = "Nature",
        EnglishWord = "forest",
        Translation = "ліс",
        Definition = "A large area covered with trees",
        ExampleSentence = "We went hiking in the forest last weekend.",
        Pronunciation = "/ˈfɔːr.ɪst/",
        Difficulty = "Beginner",
        IsPublic = false,
        Price = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Flashcard
    {
        UserId = users[1].UserId,
        Category = "Nature",
        EnglishWord = "ecosystem",
        Translation = "екосистема",
        Definition = "All the living things in an area and their environment",
        ExampleSentence = "The rainforest ecosystem is very complex and diverse.",
        Pronunciation = "/ˈiː.koʊ.sɪs.təm/",
        Difficulty = "Advanced",
        IsPublic = true,
        Price = 7.99m,
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
