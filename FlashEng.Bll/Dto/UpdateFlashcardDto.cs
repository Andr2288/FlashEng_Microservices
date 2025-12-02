using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Dto
{
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
}
