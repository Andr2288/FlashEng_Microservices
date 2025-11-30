using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Domain.models
{
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
}
