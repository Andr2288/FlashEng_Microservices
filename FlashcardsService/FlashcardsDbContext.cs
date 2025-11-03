using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FlashcardsService;

/// <summary>
/// Спрощений DbContext для флеш-карток (без системи тегів)
/// </summary>
public class FlashcardsDbContext : DbContext
{
    // DbSet - це таблиці в базі даних
    public DbSet<Flashcard> Flashcards { get; set; }

    /// <summary>
    /// Налаштування підключення до бази даних
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server=localhost;Database=flasheng_flashcards_simple;User=admin;Password=1234567890;";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
    }

    /// <summary>
    /// Створити базу даних, якщо її не існує
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync()
    {
        string serverConnectionString = "Server=localhost;User=admin;Password=1234567890;";

        using var connection = new MySqlConnection(serverConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "CREATE DATABASE IF NOT EXISTS flasheng_flashcards_simple;";

        await command.ExecuteNonQueryAsync();
        Console.WriteLine("✅ Database 'flasheng_flashcards_simple' ensured.");
    }

    /// <summary>
    /// Налаштування моделей через Fluent API
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===================================
        // Налаштування Flashcard
        // ===================================
        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.FlashcardId);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EnglishWord)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Translation)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Pronunciation)
                .HasMaxLength(100);

            entity.Property(e => e.AudioUrl)
                .HasMaxLength(500);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Difficulty)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            entity.Property(e => e.Price)
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            // Індекси для швидкого пошуку
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.EnglishWord);
            entity.HasIndex(e => e.Difficulty);
            entity.HasIndex(e => e.IsPublic);
        });

        // ===================================
        // Seed Data (початкові дані)
        // ===================================
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Додавання початкових даних в базу
    /// </summary>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Флеш-картки з категоріями як string (без тегів)
        modelBuilder.Entity<Flashcard>().HasData(
            // Безкоштовні картки (IsPublic = false)
            new Flashcard
            {
                FlashcardId = 1,
                UserId = 1,
                Category = "Food",
                EnglishWord = "apple",
                Translation = "яблуко",
                Definition = "A round fruit with red or green skin",
                ExampleSentence = "I eat an apple every day.",
                Pronunciation = "/ˈæp.əl/",
                Difficulty = "Easy",
                IsPublic = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Flashcard
            {
                FlashcardId = 2,
                UserId = 1,
                Category = "Food",
                EnglishWord = "bread",
                Translation = "хліб",
                Definition = "A basic food made from flour and water",
                ExampleSentence = "I buy fresh bread every morning.",
                Pronunciation = "/bred/",
                Difficulty = "Easy",
                IsPublic = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },

            // Платні категорії (IsPublic = true)
            new Flashcard
            {
                FlashcardId = 3,
                UserId = 1,
                Category = "Business English",
                EnglishWord = "meeting",
                Translation = "зустріч",
                Definition = "A gathering of people for discussion",
                ExampleSentence = "We have a meeting at 3 PM.",
                Pronunciation = "/ˈmiː.tɪŋ/",
                Difficulty = "Medium",
                IsPublic = true,
                Price = 29.99m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Flashcard
            {
                FlashcardId = 4,
                UserId = 1,
                Category = "Business English",
                EnglishWord = "presentation",
                Translation = "презентація",
                Definition = "A speech or talk in which something is shown",
                ExampleSentence = "I need to prepare a presentation for tomorrow.",
                Pronunciation = "/ˌprez.ənˈteɪ.ʃən/",
                Difficulty = "Hard",
                IsPublic = true,
                Price = 29.99m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Flashcard
            {
                FlashcardId = 5,
                UserId = 1,
                Category = "Travel Phrases",
                EnglishWord = "airport",
                Translation = "аеропорт",
                Definition = "A place where planes take off and land",
                ExampleSentence = "I need to be at the airport two hours early.",
                Pronunciation = "/ˈeə.pɔːt/",
                Difficulty = "Easy",
                IsPublic = true,
                Price = 19.99m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );
    }
}