using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlashEng.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Отримати всі замовлення
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all orders");
            var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Отримати замовлення по ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        /// <summary>
        /// Отримати замовлення користувача
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<OrderDto>>> GetUserOrders(int userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting orders for user: {UserId}", userId);
            var orders = await _orderService.GetUserOrdersAsync(userId, cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Створити нове замовлення
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderDto createOrderDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new order for user: {UserId}", createOrderDto.UserId);
            var orderId = await _orderService.CreateOrderAsync(createOrderDto, cancellationToken);

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
        }

        /// <summary>
        /// Створити замовлення транзакційно (з використанням збереженої процедури)
        /// </summary>
        [HttpPost("transactional")]
        public async Task<ActionResult<int>> CreateOrderTransactional([FromBody] CreateOrderDto createOrderDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new order transactionally for user: {UserId}", createOrderDto.UserId);
            var orderId = await _orderService.CreateOrderWithItemsTransactionalAsync(createOrderDto, cancellationToken);

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
        }

        /// <summary>
        /// Оновити статус замовлення
        /// </summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating status for order {OrderId} to {Status}", id, request.Status);
            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Видалити замовлення
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting order with ID: {OrderId}", id);
            var result = await _orderService.DeleteOrderAsync(id, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Отримати всі продукти
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all products");
            var products = await _orderService.GetAllProductsAsync(cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Отримати продукт по ID
        /// </summary>
        [HttpGet("products/{id:int}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var product = await _orderService.GetProductByIdAsync(id, cancellationToken);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
