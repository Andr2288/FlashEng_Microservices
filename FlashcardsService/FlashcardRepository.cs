using Microsoft.EntityFrameworkCore;

namespace FlashcardsService;

/// <summary>
/// Repository для роботи з флеш-картками через EF Core
/// </summary>
public class FlashcardRepository
{
    private readonly FlashcardsDbContext _context;

    public FlashcardRepository(FlashcardsDbContext context)
    {
        _context = context;
    }

    // ===================================
    // CRUD для категорій
    // ===================================

    /// <summary>
    /// Отримати всі категорії з кількістю карток
    /// </summary>
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.Flashcards)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    /// <summary>
    /// Створити нову категорію
    /// </summary>
    public async Task<Category> CreateCategoryAsync(string name, string description, int userId)
    {
        var category = new Category
        {
            CategoryName = name,
            Description = description,
            UserId = userId,
            IsPublic = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    // ===================================
    // CRUD для флеш-карток
    // ===================================

    /// <summary>
    /// Отримати всі картки певної категорії
    /// </summary>
    public async Task<List<Flashcard>> GetFlashcardsByCategoryAsync(int categoryId)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.CategoryId == categoryId)
            .OrderBy(f => f.EnglishWord)
            .ToListAsync();
    }

    /// <summary>
    /// Пошук карток по слову
    /// </summary>
    public async Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm)
    {
        return await _context.Flashcards
            .Include(f => f.Category)
            .Where(f => f.EnglishWord.Contains(searchTerm) ||
                       f.Translation.Contains(searchTerm))
            .ToListAsync();
    }

    /// <summary>
    /// Створити нову флеш-картку
    /// </summary>
    public async Task<Flashcard> CreateFlashcardAsync(
        int categoryId,
        string englishWord,
        string translation,
        string? definition = null,
        string? example = null,
        string difficulty = "Medium")
    {
        var flashcard = new Flashcard
        {
            CategoryId = categoryId,
            EnglishWord = englishWord,
            Translation = translation,
            Definition = definition,
            ExampleSentence = example,
            Difficulty = difficulty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Flashcards.Add(flashcard);
        await _context.SaveChangesAsync();

        return flashcard;
    }

    /// <summary>
    /// Оновити флеш-картку
    /// </summary>
    public async Task<Flashcard?> UpdateFlashcardAsync(
        int flashcardId,
        string englishWord,
        string translation,
        string? definition = null)
    {
        var flashcard = await _context.Flashcards.FindAsync(flashcardId);

        if (flashcard == null)
            return null;

        flashcard.EnglishWord = englishWord;
        flashcard.Translation = translation;
        flashcard.Definition = definition;
        flashcard.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return flashcard;
    }

    /// <summary>
    /// Видалити флеш-картку
    /// </summary>
    public async Task<bool> DeleteFlashcardAsync(int flashcardId)
    {
        var flashcard = await _context.Flashcards.FindAsync(flashcardId);

        if (flashcard == null)
            return false;

        _context.Flashcards.Remove(flashcard);
        await _context.SaveChangesAsync();

        return true;
    }

    // ===================================
    // Робота з тегами (багато-до-багатьох)
    // ===================================

    /// <summary>
    /// Отримати всі теги
    /// </summary>
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.TagName)
            .ToListAsync();
    }

    /// <summary>
    /// Додати тег до картки
    /// </summary>
    public async Task<bool> AddTagToFlashcardAsync(int flashcardId, int tagId)
    {
        // Перевіряємо, чи вже існує цей зв'язок
        var exists = await _context.FlashcardTags
            .AnyAsync(ft => ft.FlashcardId == flashcardId && ft.TagId == tagId);

        if (exists)
            return false;

        var flashcardTag = new FlashcardTag
        {
            FlashcardId = flashcardId,
            TagId = tagId,
            CreatedAt = DateTime.Now
        };

        _context.FlashcardTags.Add(flashcardTag);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Отримати картки по тегу
    /// </summary>
    public async Task<List<Flashcard>> GetFlashcardsByTagAsync(string tagName)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.FlashcardTags.Any(ft => ft.Tag.TagName == tagName))
            .ToListAsync();
    }

    // ===================================
    // Статистика
    // ===================================

    /// <summary>
    /// Отримати статистику по категоріях
    /// </summary>
    public async Task<List<CategoryStatistic>> GetCategoryStatisticsAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryStatistic
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                FlashcardCount = c.Flashcards.Count
            })
            .ToListAsync();
    }
}

/// <summary>
/// Статистика категорії
/// </summary>
public class CategoryStatistic
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int FlashcardCount { get; set; }
}