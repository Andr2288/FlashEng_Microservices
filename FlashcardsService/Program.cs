using FlashcardsService;
using Microsoft.EntityFrameworkCore;

namespace FlashEngFlashcards
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Flashcards Service (БЕЗ ТЕГІВ)");
            Console.WriteLine("  Flashcards + EF Core");
            Console.WriteLine("===========================================\n");

            try
            {
                using var context = new FlashcardsDbContext();

                Console.WriteLine("Creating/updating database...");
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Database ready!\n");

                var repository = new FlashcardRepository(context);

                // 1. Показати всі категорії
                Console.WriteLine("--- All Categories ---");
                var categories = await repository.GetAllCategoriesAsync();
                foreach (var category in categories)
                {
                    Console.WriteLine($"- {category}");
                }

                // 2. Показати категорії, доступні для покупки
                Console.WriteLine("\n--- Available Categories for Purchase ---");
                var availableCategories = await repository.GetAvailableCategoriesAsync();
                foreach (var category in availableCategories)
                {
                    Console.WriteLine($"📦 {category.Category}");
                    Console.WriteLine($"   Cards: {category.FlashcardCount} | Price: ${category.Price}");
                    Console.WriteLine($"   Difficulties: {string.Join(", ", category.Difficulties)}");
                    Console.WriteLine($"   {category.Description}");
                }

                // 3. Показати всі картки категорії Food
                Console.WriteLine("\n--- Flashcards in 'Food' Category ---");
                var foodCards = await repository.GetFlashcardsByCategoryAsync("Food");
                foreach (var card in foodCards)
                {
                    Console.WriteLine($"- {card.EnglishWord} = {card.Translation}");
                    Console.WriteLine($"  Definition: {card.Definition}");
                    Console.WriteLine($"  Difficulty: {card.Difficulty}");
                    Console.WriteLine($"  Public: {card.IsPublic} | Price: {card.Price:C}");
                }

                // 4. Створити нову безкоштовну картку
                Console.WriteLine("\n--- Creating Free Flashcard ---");
                var newFreeCard = await repository.CreateFlashcardAsync(
                    userId: 1,
                    category: "Food",
                    englishWord: "pizza",
                    translation: "піца",
                    definition: "A dish of Italian origin",
                    example: "I love eating pizza on weekends.",
                    difficulty: "Easy",
                    isPublic: false
                );
                Console.WriteLine($"Created free flashcard: {newFreeCard.EnglishWord} (ID: {newFreeCard.FlashcardId})");

                // 5. Створити нову платну картку
                Console.WriteLine("\n--- Creating Premium Flashcard ---");
                var newPremiumCard = await repository.CreateFlashcardAsync(
                    userId: 1,
                    category: "Advanced Business",
                    englishWord: "stakeholder",
                    translation: "зацікавлена сторона",
                    definition: "A person with an interest in a business",
                    example: "We need to inform all stakeholders about the changes.",
                    difficulty: "Hard",
                    isPublic: true,
                    price: 39.99m
                );
                Console.WriteLine($"Created premium flashcard: {newPremiumCard.EnglishWord} (ID: {newPremiumCard.FlashcardId})");

                // 6. Пошук карток
                Console.WriteLine("\n--- Searching for 'apple' ---");
                var searchResults = await repository.SearchFlashcardsAsync("apple");
                foreach (var card in searchResults)
                {
                    Console.WriteLine($"Found: {card.EnglishWord} in category '{card.Category}'");
                }

                // 7. Показати картки користувача
                Console.WriteLine("\n--- User's Flashcards (User ID = 1) ---");
                var userCards = await repository.GetUserFlashcardsAsync(1);
                foreach (var card in userCards.Take(5)) // показати перші 5
                {
                    Console.WriteLine($"- {card.EnglishWord} ({card.Category}) - {(card.IsPublic ? "Public" : "Private")}");
                }

                // 8. Статистика категорій
                Console.WriteLine("\n--- Category Statistics ---");
                var stats = await repository.GetCategoryStatisticsAsync();
                foreach (var stat in stats)
                {
                    Console.WriteLine($"📊 {stat.Category}: {stat.FlashcardCount} total | {stat.PublicCount} public | Avg price: {stat.AveragePrice:C}");
                }

                // 9. Популярні категорії
                Console.WriteLine("\n--- Top 3 Popular Categories ---");
                var popularCategories = await repository.GetPopularCategoriesAsync(3);
                foreach (var category in popularCategories)
                {
                    Console.WriteLine($"🔥 {category.Category}: {category.FlashcardCount} cards");
                }

                // 10. Картки по складності
                Console.WriteLine("\n--- Easy Flashcards ---");
                var easyCards = await repository.GetFlashcardsByDifficultyAsync("Easy");
                foreach (var card in easyCards.Take(3))
                {
                    Console.WriteLine($"- {card.EnglishWord} ({card.Category})");
                }

                // 11. Оновити картку
                Console.WriteLine("\n--- Updating Flashcard ---");
                var updated = await repository.UpdateFlashcardAsync(
                    flashcardId: 1,
                    englishWord: "apple",
                    translation: "яблуко (червоне чи зелене)",
                    definition: "A round, sweet fruit that grows on trees and is good for health"
                );
                if (updated != null)
                    Console.WriteLine($"Updated: {updated.EnglishWord}");

                Console.WriteLine("\n===========================================");
                Console.WriteLine("  Операції виконано успішно!");
                Console.WriteLine("  Спрощена модель: Flashcards (БЕЗ ТЕГІВ)");
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