using System;

namespace FlashEng.Domain.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class UserSettings
    {
        public int SettingsId { get; set; }
        public int UserId { get; set; }
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en";
        public bool NotificationsEnabled { get; set; } = true;
    }

    public class Flashcard
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
        public string Difficulty { get; set; } = "Medium";
        public bool IsPublic { get; set; } = false;
        public decimal? Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; }
    }
}
