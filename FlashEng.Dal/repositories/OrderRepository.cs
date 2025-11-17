using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Models;
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
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";
            var orders = await connection.QueryAsync<Order>(sql);
            return orders.ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Orders WHERE OrderId = @OrderId";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderId = orderId });
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";
            var orders = await connection.QueryAsync<Order>(sql, new { UserId = userId });
            return orders.ToList();
        }

        public async Task<int> CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            INSERT INTO Orders (UserId, TotalAmount, Status)
            VALUES (@UserId, @TotalAmount, @Status);
            SELECT LAST_INSERT_ID();";

            return await connection.QuerySingleAsync<int>(sql, order);
        }

        public async Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            UPDATE Orders 
            SET TotalAmount = @TotalAmount, Status = @Status
            WHERE OrderId = @OrderId";

            var rowsAffected = await connection.ExecuteAsync(sql, order);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "DELETE FROM Orders WHERE OrderId = @OrderId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId });
            return rowsAffected > 0;
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            var items = await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
            return items.ToList();
        }

        public async Task<int> CreateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal);
            SELECT LAST_INSERT_ID();";

            return await connection.QuerySingleAsync<int>(sql, orderItem);
        }

        public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Products WHERE IsAvailable = TRUE ORDER BY Name";
            var products = await connection.QueryAsync<Product>(sql);
            return products.ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Products WHERE ProductId = @ProductId";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductId = productId });
        }

        // Використання збереженої процедури через ADO.NET
        public async Task<int> CreateOrderWithItemsAsync(int userId, List<(int productId, int quantity)> items, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateOrderWithItems";

            var productIds = string.Join(",", items.Select(i => i.productId));
            var quantities = string.Join(",", items.Select(i => i.quantity));

            command.Parameters.AddWithValue("p_UserId", userId);
            command.Parameters.AddWithValue("p_ProductIds", productIds);
            command.Parameters.AddWithValue("p_Quantities", quantities);

            var orderIdParam = new MySqlParameter("p_OrderId", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(orderIdParam);

            await command.ExecuteNonQueryAsync(cancellationToken);

            return Convert.ToInt32(orderIdParam.Value);
        }
    }
}
