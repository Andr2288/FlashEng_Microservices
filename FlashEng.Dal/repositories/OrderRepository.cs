using FlashEng.Dal.Context;
using FlashEng.Dal.Interfaces;
using FlashEng.Dal.repositories;
using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Dal.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            order.OrderDate = DateTime.UtcNow;
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return order.OrderId;
        }

        public async Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(order);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order = await GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null) return false;

            _context.Orders.Remove(order);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        // EAGER LOADING example - включаємо Product дані
        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product) // Eager Loading - завантажуємо Product разом з OrderItem
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CreateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _context.OrderItems.AddAsync(orderItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return orderItem.OrderItemId;
        }

        public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        // Simplified version without stored procedure for now
        public async Task<int> CreateOrderWithItemsAsync(int userId, List<(int productId, int quantity)> items, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create order
                var order = new Order
                {
                    UserId = userId,
                    Status = "Pending",
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = 0
                };

                await _context.Orders.AddAsync(order, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                decimal totalAmount = 0;

                // Create order items
                foreach (var (productId, quantity) in items)
                {
                    var product = await GetProductByIdAsync(productId, cancellationToken);
                    if (product == null || !product.IsAvailable)
                    {
                        throw new InvalidOperationException($"Product {productId} not available");
                    }

                    var lineTotal = product.Price * quantity;
                    totalAmount += lineTotal;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    };

                    await _context.OrderItems.AddAsync(orderItem, cancellationToken);
                }

                // Update total amount
                order.TotalAmount = totalAmount;
                _context.Orders.Update(order);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return order.OrderId;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
