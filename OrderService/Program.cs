namespace OrderService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FlashEng - Simple Order Service");
            Console.WriteLine("  4 Таблиці + Зв'язки + 4 Процедури");
            Console.WriteLine("===========================================\n");

            try
            {
                // Створюємо базу даних та таблиці
                await DatabaseConfig.EnsureDatabaseCreatedAsync();
                await DatabaseConfig.EnsureTablesCreatedAsync();

                var repository = new OrderRepository();

                // 1. Показати всі продукти
                Console.WriteLine("\n--- 📦 PRODUCTS ---");
                var products = await repository.GetAllProductsAsync();
                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.ProductId} | {product.Name} | ${product.Price}");
                }

                // 2. Показати всі замовлення
                Console.WriteLine("\n--- 📋 ORDERS ---");
                var orders = await repository.GetAllOrdersAsync();
                foreach (var order in orders)
                {
                    Console.WriteLine($"ID: {order.OrderId} | User: {order.UserId} | Status: {order.Status} | Total: ${order.TotalAmount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
    }
}