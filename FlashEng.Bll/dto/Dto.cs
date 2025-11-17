using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserSettingsDto
    {
        public int SettingsId { get; set; }
        public int UserId { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool NotificationsEnabled { get; set; }
    }

    public class FlashcardDto
    {
        public int FlashcardId { get; set; }
        public int UserId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string EnglishWord { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Pronunciation { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public decimal? Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFlashcardDto
    {
        public int UserId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string EnglishWord { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Pronunciation { get; set; }
        public string Difficulty { get; set; } = "Medium";
        public bool IsPublic { get; set; } = false;
        public decimal? Price { get; set; }
    }

    public class UpdateFlashcardDto
    {
        public string Category { get; set; } = string.Empty;
        public string EnglishWord { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Pronunciation { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public decimal? Price { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
