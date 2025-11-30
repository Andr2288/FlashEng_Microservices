using FlashEng.Domain.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Interfaces
{
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
}
