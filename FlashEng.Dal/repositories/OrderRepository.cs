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
            var orders = new List<Order>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Orders ORDER BY OrderDate DESC";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32("OrderId"),
                    UserId = reader.GetInt32("UserId"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    Status = reader.GetString("Status"),
                    OrderDate = reader.GetDateTime("OrderDate")
                });
            }

            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Orders WHERE OrderId = @OrderId";
            command.Parameters.AddWithValue("@OrderId", orderId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new Order
                {
                    OrderId = reader.GetInt32("OrderId"),
                    UserId = reader.GetInt32("UserId"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    Status = reader.GetString("Status"),
                    OrderDate = reader.GetDateTime("OrderDate")
                };
            }

            return null;
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
        {
            var orders = new List<Order>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32("OrderId"),
                    UserId = reader.GetInt32("UserId"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    Status = reader.GetString("Status"),
                    OrderDate = reader.GetDateTime("OrderDate")
                });
            }

            return orders;
        }

        public async Task<int> CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Orders (UserId, TotalAmount, Status)
            VALUES (@UserId, @TotalAmount, @Status);
            SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@UserId", order.UserId);
            command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
            command.Parameters.AddWithValue("@Status", order.Status);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE Orders 
            SET TotalAmount = @TotalAmount, Status = @Status
            WHERE OrderId = @OrderId";

            command.Parameters.AddWithValue("@OrderId", order.OrderId);
            command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
            command.Parameters.AddWithValue("@Status", order.Status);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Orders WHERE OrderId = @OrderId";
            command.Parameters.AddWithValue("@OrderId", orderId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var items = new List<OrderItem>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            command.Parameters.AddWithValue("@OrderId", orderId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new OrderItem
                {
                    OrderItemId = reader.GetInt32("OrderItemId"),
                    OrderId = reader.GetInt32("OrderId"),
                    ProductId = reader.GetInt32("ProductId"),
                    Quantity = reader.GetInt32("Quantity"),
                    UnitPrice = reader.GetDecimal("UnitPrice"),
                    LineTotal = reader.GetDecimal("LineTotal")
                });
            }

            return items;
        }

        public async Task<int> CreateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal);
            SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@OrderId", orderItem.OrderId);
            command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
            command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
            command.Parameters.AddWithValue("@UnitPrice", orderItem.UnitPrice);
            command.Parameters.AddWithValue("@LineTotal", orderItem.LineTotal);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            var products = new List<Product>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Products WHERE IsAvailable = TRUE ORDER BY Name";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                products.Add(new Product
                {
                    ProductId = reader.GetInt32("ProductId"),
                    Name = reader.GetString("Name"),
                    Price = reader.GetDecimal("Price"),
                    IsAvailable = reader.GetBoolean("IsAvailable"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Products WHERE ProductId = @ProductId";
            command.Parameters.AddWithValue("@ProductId", productId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new Product
                {
                    ProductId = reader.GetInt32("ProductId"),
                    Name = reader.GetString("Name"),
                    Price = reader.GetDecimal("Price"),
                    IsAvailable = reader.GetBoolean("IsAvailable"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

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
