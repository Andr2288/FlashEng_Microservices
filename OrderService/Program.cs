using OrderService;

namespace FlashEngOrders
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Order Service (РОЗШИРЕНИЙ)");
            Console.WriteLine("  4 Таблиці + Зв'язки + 6 Процедур");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних та таблиці
                await DatabaseConfig.EnsureDatabaseCreatedAsync();
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new OrderRepository();

                // 1. Показати Products (база для зв'язків)
                Console.WriteLine("\n--- 📦 ALL PRODUCTS ---");
                var products = await repository.GetAllProductsAsync();
                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.ProductId} | {product.CategoryName} | ${product.Price} | {(product.IsAvailable ? "✅" : "❌")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}