using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FlashcardsService;

/// <summary>
/// DbContext для роботи з базою даних Flashcards через Entity Framework Core
/// </summary>
public class FlashcardsDbContext : DbContext
{
    // DbSet - це таблиці в базі даних
    public DbSet<Category> Categories { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<FlashcardTag> FlashcardTags { get; set; }

    /// <summary>
    /// Налаштування підключення до бази даних
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server=localhost;Database=flasheng_flashcards;User=admin;Password=1234567890;";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
    }

    /// <summary>
    /// Налаштування моделей через Fluent API
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===================================
        // Налаштування Category
        // ===================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IconName)
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            // Індекси для швидкого пошуку
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.CategoryName);
        });

        // ===================================
        // Налаштування Flashcard
        // ===================================
        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.FlashcardId);

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

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            // Зв'язок 1:N з Category (одна категорія має багато карток)
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Flashcards)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Індекси
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.EnglishWord);
            entity.HasIndex(e => e.Difficulty);
        });

        // ===================================
        // Налаштування Tag
        // ===================================
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId);

            entity.Property(e => e.TagName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Унікальний індекс - не може бути двох однакових тегів
            entity.HasIndex(e => e.TagName)
                .IsUnique();
        });

        // ===================================
        // Налаштування FlashcardTag (зв'язок M:N)
        // ===================================
        modelBuilder.Entity<FlashcardTag>(entity =>
        {
            entity.HasKey(e => e.FlashcardTagId);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Зв'язок багато-до-багатьох
            entity.HasOne(ft => ft.Flashcard)
                .WithMany(f => f.FlashcardTags)
                .HasForeignKey(ft => ft.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ft => ft.Tag)
                .WithMany(t => t.FlashcardTags)
                .HasForeignKey(ft => ft.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Унікальний індекс - картка не може мати один тег двічі
            entity.HasIndex(e => new { e.FlashcardId, e.TagId })
                .IsUnique();
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
        // Категорії
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Food", Description = "Common food vocabulary", IconName = "🍔", UserId = 1, IsPublic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Category { CategoryId = 2, CategoryName = "Travel", Description = "Words for traveling", IconName = "✈️", UserId = 1, IsPublic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        );

        // Теги
        modelBuilder.Entity<Tag>().HasData(
            new Tag { TagId = 1, TagName = "Beginner", CreatedAt = DateTime.Now },
            new Tag { TagId = 2, TagName = "Noun", CreatedAt = DateTime.Now },
            new Tag { TagId = 3, TagName = "Common", CreatedAt = DateTime.Now }
        );

        // Флеш-картки
        modelBuilder.Entity<Flashcard>().HasData(
            new Flashcard
            {
                FlashcardId = 1,
                CategoryId = 1,
                EnglishWord = "apple",
                Translation = "яблуко",
                Definition = "A round fruit with red or green skin",
                ExampleSentence = "I eat an apple every day.",
                Pronunciation = "/ˈæp.əl/",
                Difficulty = "Easy",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Flashcard
            {
                FlashcardId = 2,
                CategoryId = 2,
                EnglishWord = "airport",
                Translation = "аеропорт",
                Definition = "A place where planes take off and land",
                ExampleSentence = "I need to be at the airport two hours early.",
                Pronunciation = "/ˈeə.pɔːt/",
                Difficulty = "Easy",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );

        // Зв'язки FlashcardTags
        modelBuilder.Entity<FlashcardTag>().HasData(
            new FlashcardTag { FlashcardTagId = 1, FlashcardId = 1, TagId = 1, CreatedAt = DateTime.Now },
            new FlashcardTag { FlashcardTagId = 2, FlashcardId = 1, TagId = 2, CreatedAt = DateTime.Now },
            new FlashcardTag { FlashcardTagId = 3, FlashcardId = 2, TagId = 1, CreatedAt = DateTime.Now }
        );
    }
}