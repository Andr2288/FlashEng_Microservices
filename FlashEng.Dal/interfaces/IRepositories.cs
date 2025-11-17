using FlashEng.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Dal.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserSettings?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> CreateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
    }

    public interface IFlashcardRepository
    {
        Task<List<Flashcard>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default);
        Task<List<Flashcard>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Flashcard?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<int> CreateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default);
        Task<bool> UpdateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default);
        Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    }

    public interface IOrderRepository
    {
        Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> CreateOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
        Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default);
        Task<int> CreateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<int> CreateOrderWithItemsAsync(int userId, List<(int productId, int quantity)> items, CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IFlashcardRepository Flashcards { get; }
        IOrderRepository Orders { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
