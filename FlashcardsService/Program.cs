using FlashcardsService;
using Microsoft.EntityFrameworkCore;

namespace FlashEngFlashcards
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Flashcards Service (EF Core)");
            Console.WriteLine("  Code First + Fluent API");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо контекст бази даних
                using var context = new FlashcardsDbContext();

                // ВАЖЛИВО: Створюємо базу даних, якщо її немає
                Console.WriteLine("Creating/updating database...");
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Database ready!\n");

                var repository = new FlashcardRepository(context);

                // 1. Показати всі категорії
                Console.WriteLine("--- All Categories ---");
                var categories = await repository.GetAllCategoriesAsync();

                foreach (var category in categories)
                {
                    Console.WriteLine($"ID: {category.CategoryId} | {category.CategoryName} | Cards: {category.Flashcards.Count}");
                }

                // 2. Створити нову категорію (з унікальною назвою)
                Console.WriteLine("\n--- Creating New Category ---");
                string categoryName = $"Test Category {DateTime.Now:HHmmss}";
                var newCategory = await repository.CreateCategoryAsync(
                    categoryName,
                    "Test category for demonstration",
                    1
                );
                Console.WriteLine($"Created category: {newCategory.CategoryName} (ID: {newCategory.CategoryId})");

                // 3. Показати всі картки категорії Food
                Console.WriteLine("\n--- Flashcards in 'Food' Category ---");
                var foodCards = await repository.GetFlashcardsByCategoryAsync(1);

                foreach (var card in foodCards)
                {
                    Console.WriteLine($"- {card.EnglishWord} = {card.Translation}");
                    Console.WriteLine($"  Definition: {card.Definition}");
                    Console.WriteLine($"  Tags: {string.Join(", ", card.FlashcardTags.Select(ft => ft.Tag.TagName))}");
                }

                // 4. Створити нову флеш-картку
                Console.WriteLine("\n--- Creating New Flashcard ---");
                var newCard = await repository.CreateFlashcardAsync(
                    categoryId: 1,
                    englishWord: "pizza",
                    translation: "піца",
                    definition: "A dish of Italian origin",
                    example: "I love eating pizza on weekends.",
                    difficulty: "Easy"
                );
                Console.WriteLine($"Created flashcard: {newCard.EnglishWord} (ID: {newCard.FlashcardId})");

                // 5. Пошук карток
                Console.WriteLine("\n--- Searching for 'apple' ---");
                var searchResults = await repository.SearchFlashcardsAsync("apple");

                foreach (var card in searchResults)
                {
                    Console.WriteLine($"Found: {card.EnglishWord} in category {card.Category.CategoryName}");
                }

                // 6. Оновити картку
                Console.WriteLine("\n--- Updating Flashcard (ID = 1) ---");
                var updated = await repository.UpdateFlashcardAsync(
                    flashcardId: 1,
                    englishWord: "apple",
                    translation: "яблуко (фрукт)",
                    definition: "A round, sweet fruit that grows on trees"
                );

                if (updated != null)
                    Console.WriteLine($"Updated: {updated.EnglishWord}");

                // 7. Показати всі теги
                Console.WriteLine("\n--- All Tags ---");
                var tags = await repository.GetAllTagsAsync();

                foreach (var tag in tags)
                {
                    Console.WriteLine($"- {tag.TagName}");
                }

                // 8. Додати тег до картки
                Console.WriteLine("\n--- Adding Tag to Flashcard ---");
                bool tagAdded = await repository.AddTagToFlashcardAsync(
                    flashcardId: newCard.FlashcardId,
                    tagId: 1 // Beginner
                );
                Console.WriteLine($"Tag added: {tagAdded}");

                // 9. Отримати картки по тегу
                Console.WriteLine("\n--- Flashcards with 'Beginner' Tag ---");
                var beginnerCards = await repository.GetFlashcardsByTagAsync("Beginner");

                foreach (var card in beginnerCards)
                {
                    Console.WriteLine($"- {card.EnglishWord}");
                }

                // 10. Статистика
                Console.WriteLine("\n--- Category Statistics ---");
                var stats = await repository.GetCategoryStatisticsAsync();

                foreach (var stat in stats)
                {
                    Console.WriteLine($"ID: {stat.CategoryId} | {stat.CategoryName}: {stat.FlashcardCount} cards");
                }

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  All operations completed successfully!");
                Console.WriteLine("  Used: EF Core + Code First + Async");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}