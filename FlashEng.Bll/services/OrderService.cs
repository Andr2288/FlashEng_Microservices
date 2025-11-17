using AutoMapper;
using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Exceptions;
using FlashEng.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Bll.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync(cancellationToken);
            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = _mapper.Map<OrderDto>(order);
                var orderItems = await _unitOfWork.Orders.GetOrderItemsAsync(order.OrderId, cancellationToken);
                orderDto.Items = _mapper.Map<List<OrderItemDto>>(orderItems);
                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new ValidationException("Order ID must be positive");

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null)
                return null;

            var orderDto = _mapper.Map<OrderDto>(order);
            var orderItems = await _unitOfWork.Orders.GetOrderItemsAsync(orderId, cancellationToken);
            orderDto.Items = _mapper.Map<List<OrderItemDto>>(orderItems);

            return orderDto;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var orders = await _unitOfWork.Orders.GetUserOrdersAsync(userId, cancellationToken);
            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = _mapper.Map<OrderDto>(order);
                var orderItems = await _unitOfWork.Orders.GetOrderItemsAsync(order.OrderId, cancellationToken);
                orderDto.Items = _mapper.Map<List<OrderItemDto>>(orderItems);
                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task<int> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default)
        {
            // Валідація
            if (createOrderDto.UserId <= 0)
                throw new ValidationException("User ID must be positive");

            if (!createOrderDto.Items.Any())
                throw new ValidationException("Order must have at least one item");

            // Перевірка існування користувача
            var user = await _unitOfWork.Users.GetUserByIdAsync(createOrderDto.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", createOrderDto.UserId);

            // Створення замовлення
            var order = _mapper.Map<Order>(createOrderDto);
            var orderId = await _unitOfWork.Orders.CreateOrderAsync(order, cancellationToken);

            // Додавання позицій
            decimal totalAmount = 0;
            foreach (var itemDto in createOrderDto.Items)
            {
                var product = await _unitOfWork.Orders.GetProductByIdAsync(itemDto.ProductId, cancellationToken);
                if (product == null)
                    throw new NotFoundException("Product", itemDto.ProductId);

                if (!product.IsAvailable)
                    throw new BusinessConflictException($"Product {product.Name} is not available");

                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * itemDto.Quantity
                };

                await _unitOfWork.Orders.CreateOrderItemAsync(orderItem, cancellationToken);
                totalAmount += orderItem.LineTotal;
            }

            // Оновлення загальної суми
            order.OrderId = orderId;
            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.UpdateOrderAsync(order, cancellationToken);

            return orderId;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new ValidationException("Order ID must be positive");

            if (string.IsNullOrWhiteSpace(status))
                throw new ValidationException("Status cannot be empty");

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null)
                throw new NotFoundException("Order", orderId);

            var validStatuses = new[] { "Pending", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                throw new ValidationException("Invalid status value");

            order.Status = status;
            return await _unitOfWork.Orders.UpdateOrderAsync(order, cancellationToken);
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new ValidationException("Order ID must be positive");

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId, cancellationToken);
            if (order == null)
                throw new NotFoundException("Order", orderId);

            if (order.Status == "Completed")
                throw new BusinessConflictException("Cannot delete completed order");

            return await _unitOfWork.Orders.DeleteOrderAsync(orderId, cancellationToken);
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.Orders.GetAllProductsAsync(cancellationToken);
            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new ValidationException("Product ID must be positive");

            var product = await _unitOfWork.Orders.GetProductByIdAsync(productId, cancellationToken);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        // Транзакційний метод з використанням UoW
        public async Task<int> CreateOrderWithItemsTransactionalAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default)
        {
            // Валідація
            if (createOrderDto.UserId <= 0)
                throw new ValidationException("User ID must be positive");

            if (!createOrderDto.Items.Any())
                throw new ValidationException("Order must have at least one item");

            // Перевірка існування користувача
            var user = await _unitOfWork.Users.GetUserByIdAsync(createOrderDto.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", createOrderDto.UserId);

            // Початок транзакції
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Підготовка даних для збереженої процедури
                var items = new List<(int productId, int quantity)>();

                foreach (var itemDto in createOrderDto.Items)
                {
                    var product = await _unitOfWork.Orders.GetProductByIdAsync(itemDto.ProductId, cancellationToken);
                    if (product == null)
                        throw new NotFoundException("Product", itemDto.ProductId);

                    if (!product.IsAvailable)
                        throw new BusinessConflictException($"Product {product.Name} is not available");

                    items.Add((itemDto.ProductId, itemDto.Quantity));
                }

                // Виклик збереженої процедури
                var orderId = await _unitOfWork.Orders.CreateOrderWithItemsAsync(createOrderDto.UserId, items, cancellationToken);

                // Підтвердження транзакції
                await _unitOfWork.CommitAsync(cancellationToken);

                return orderId;
            }
            catch
            {
                // Відкат транзакції у разі помилки
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
