using FlashEng.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Interfaces
{
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
