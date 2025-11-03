namespace FlashcardsService;

/// <summary>
/// Спрощена модель флеш-картки (без Categories таблиці)
/// </summary>
public class Flashcard
{
    public int FlashcardId { get; set; }
    public int UserId { get; set; } // Власник картки
    public string Category { get; set; } = string.Empty; // Тепер просто string
    public string EnglishWord { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? ExampleSentence { get; set; }
    public string? Pronunciation { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
    public bool IsPublic { get; set; } = false; // Чи доступна для покупки
    public decimal? Price { get; set; } // Ціна за категорію (якщо IsPublic = true)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Навігаційна властивість (M:N)
    public List<FlashcardTag> FlashcardTags { get; set; } = new();
}

/// <summary>
/// Модель тегу (залишається без змін)
/// </summary>
public class Tag
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Навігаційна властивість (M:N)
    public List<FlashcardTag> FlashcardTags { get; set; } = new();
}

/// <summary>
/// Зв'язок Many-to-Many між Flashcards і Tags
/// </summary>
public class FlashcardTag
{
    public int FlashcardTagId { get; set; }
    public int FlashcardId { get; set; }
    public int TagId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навігаційні властивості
    public Flashcard Flashcard { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

/// <summary>
/// Статистика по категоріях (для звітів)
/// </summary>
public class CategoryStatistic
{
    public string Category { get; set; } = string.Empty;
    public int FlashcardCount { get; set; }
    public int PublicCount { get; set; }
    public decimal? AveragePrice { get; set; }
}

/// <summary>
/// Модель для показу категорій, що доступні для покупки
/// </summary>
public class AvailableCategory
{
    public string Category { get; set; } = string.Empty;
    public int FlashcardCount { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string[] Difficulties { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
}