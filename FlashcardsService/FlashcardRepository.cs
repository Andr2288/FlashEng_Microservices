using Microsoft.EntityFrameworkCore;

namespace FlashcardsService;

/// <summary>
/// Спрощений Repository для роботи з флеш-картками
/// </summary>
public class FlashcardRepository
{
    private readonly FlashcardsDbContext _context;

    public FlashcardRepository(FlashcardsDbContext context)
    {
        _context = context;
    }

    // ===================================
    // CRUD для флеш-карток
    // ===================================

    /// <summary>
    /// Отримати всі картки
    /// </summary>
    public async Task<List<Flashcard>> GetAllFlashcardsAsync()
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .OrderBy(f => f.Category)
            .ThenBy(f => f.EnglishWord)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати картки користувача
    /// </summary>
    public async Task<List<Flashcard>> GetUserFlashcardsAsync(int userId)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Category)
            .ThenBy(f => f.EnglishWord)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати картки по категорії
    /// </summary>
    public async Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.Category == category)
            .OrderBy(f => f.EnglishWord)
            .ToListAsync();
    }

    /// <summary>
    /// Пошук карток по слову
    /// </summary>
    public async Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.EnglishWord.Contains(searchTerm) ||
                       f.Translation.Contains(searchTerm) ||
                       f.Category.Contains(searchTerm))
            .ToListAsync();
    }

    /// <summary>
    /// Створити нову флеш-картку
    /// </summary>
    public async Task<Flashcard> CreateFlashcardAsync(
        int userId,
        string category,
        string englishWord,
        string translation,
        string? definition = null,
        string? example = null,
        string difficulty = "Medium",
        bool isPublic = false,
        decimal? price = null)
    {
        var flashcard = new Flashcard
        {
            UserId = userId,
            Category = category,
            EnglishWord = englishWord,
            Translation = translation,
            Definition = definition,
            ExampleSentence = example,
            Difficulty = difficulty,
            IsPublic = isPublic,
            Price = price,
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
    // Робота з категоріями (як string)
    // ===================================

    /// <summary>
    /// Отримати всі унікальні категорії
    /// </summary>
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        return await _context.Flashcards
            .Select(f => f.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати категорії, доступні для покупки
    /// </summary>
    public async Task<List<AvailableCategory>> GetAvailableCategoriesAsync()
    {
        // Спочатку отримуємо всі публічні картки з цінами
        var publicFlashcards = await _context.Flashcards
            .Where(f => f.IsPublic && f.Price.HasValue)
            .ToListAsync();

        // Потім групуємо їх в пам'яті
        return publicFlashcards
            .GroupBy(f => f.Category)
            .Select(g => new AvailableCategory
            {
                Category = g.Key,
                FlashcardCount = g.Count(),
                Price = g.First().Price!.Value,
                Description = $"Professional {g.Key} vocabulary with {g.Count()} flashcards",
                Difficulties = g.Select(f => f.Difficulty).Distinct().ToArray(),
                CreatedAt = g.Min(f => f.CreatedAt)
            })
            .OrderBy(c => c.Category)
            .ToList();
    }

    // ===================================
    // Робота з тегами (без змін)
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
        // Отримуємо всі картки
        var allFlashcards = await _context.Flashcards.ToListAsync();

        // Групуємо в пам'яті
        return allFlashcards
            .GroupBy(f => f.Category)
            .Select(g => new CategoryStatistic
            {
                Category = g.Key,
                FlashcardCount = g.Count(),
                PublicCount = g.Count(f => f.IsPublic),
                AveragePrice = g.Where(f => f.Price.HasValue).Any()
                    ? g.Where(f => f.Price.HasValue).Average(f => f.Price!.Value)
                    : null
            })
            .OrderBy(s => s.Category)
            .ToList();
    }

    /// <summary>
    /// Отримати популярні категорії
    /// </summary>
    public async Task<List<CategoryStatistic>> GetPopularCategoriesAsync(int topCount = 5)
    {
        // Отримуємо всі картки
        var allFlashcards = await _context.Flashcards.ToListAsync();

        // Групуємо в пам'яті та сортуємо
        return allFlashcards
            .GroupBy(f => f.Category)
            .Select(g => new CategoryStatistic
            {
                Category = g.Key,
                FlashcardCount = g.Count(),
                PublicCount = g.Count(f => f.IsPublic),
                AveragePrice = g.Where(f => f.Price.HasValue).Any()
                    ? g.Where(f => f.Price.HasValue).Average(f => f.Price!.Value)
                    : null
            })
            .OrderByDescending(s => s.FlashcardCount)
            .Take(topCount)
            .ToList();
    }

    /// <summary>
    /// Отримати картки по складності
    /// </summary>
    public async Task<List<Flashcard>> GetFlashcardsByDifficultyAsync(string difficulty)
    {
        return await _context.Flashcards
            .Include(f => f.FlashcardTags)
            .ThenInclude(ft => ft.Tag)
            .Where(f => f.Difficulty == difficulty)
            .OrderBy(f => f.Category)
            .ThenBy(f => f.EnglishWord)
            .ToListAsync();
    }
}