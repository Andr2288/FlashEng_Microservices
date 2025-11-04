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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}