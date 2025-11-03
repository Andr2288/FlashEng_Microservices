namespace FlashcardsService;

/// <summary>
/// Спрощена модель флеш-картки (без системи тегів)
/// </summary>
public class Flashcard
{
    public int FlashcardId { get; set; }
    public int UserId { get; set; } // Власник картки
    public string Category { get; set; } = string.Empty;
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