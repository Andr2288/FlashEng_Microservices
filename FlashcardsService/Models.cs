namespace FlashcardsService;

/// <summary>
/// Модель категорії
/// </summary>
public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public int UserId { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Навігаційна властивість (1:N - одна категорія має багато карток)
    public List<Flashcard> Flashcards { get; set; } = new();
}

/// <summary>
/// Модель флеш-картки
/// </summary>
public class Flashcard
{
    public int FlashcardId { get; set; }
    public int CategoryId { get; set; }
    public string EnglishWord { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? ExampleSentence { get; set; }
    public string? Pronunciation { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Навігаційні властивості
    public Category Category { get; set; } = null!;
    public List<FlashcardTag> FlashcardTags { get; set; } = new();
}

/// <summary>
/// Модель тегу
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
/// Проміжна таблиця для зв'язку багато-до-багатьох (M:N)
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