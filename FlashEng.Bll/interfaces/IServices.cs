using FlashEng.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Bll.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<int> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(int userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserSettingsDto?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserSettingsAsync(int userId, UserSettingsDto settingsDto, CancellationToken cancellationToken = default);
    }

    public interface IFlashcardService
    {
        Task<List<FlashcardDto>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default);
        Task<FlashcardDto?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<int> CreateFlashcardAsync(CreateFlashcardDto createFlashcardDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateFlashcardAsync(int flashcardId, UpdateFlashcardDto updateFlashcardDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    }

    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
        Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<int> CreateOrderWithItemsTransactionalAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default);
    }   
}
