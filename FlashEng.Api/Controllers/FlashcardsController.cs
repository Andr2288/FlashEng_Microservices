using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlashEng.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlashcardsController : ControllerBase
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ILogger<FlashcardsController> _logger;

        public FlashcardsController(IFlashcardService flashcardService, ILogger<FlashcardsController> logger)
        {
            _flashcardService = flashcardService;
            _logger = logger;
        }

        /// <summary>
        /// Отримати всі флешкартки
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FlashcardDto>>> GetAllFlashcards(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all flashcards");
            var flashcards = await _flashcardService.GetAllFlashcardsAsync(cancellationToken);
            return Ok(flashcards);
        }

        /// <summary>
        /// Отримати флешкартки користувача
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<FlashcardDto>>> GetUserFlashcards(int userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting flashcards for user: {UserId}", userId);
            var flashcards = await _flashcardService.GetUserFlashcardsAsync(userId, cancellationToken);
            return Ok(flashcards);
        }

        /// <summary>
        /// Отримати флешкартку по ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FlashcardDto>> GetFlashcardById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting flashcard with ID: {FlashcardId}", id);
            var flashcard = await _flashcardService.GetFlashcardByIdAsync(id, cancellationToken);

            if (flashcard == null)
                return NotFound();

            return Ok(flashcard);
        }

        /// <summary>
        /// Отримати флешкартки по категорії
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<FlashcardDto>>> GetFlashcardsByCategory(string category, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting flashcards for category: {Category}", category);
            var flashcards = await _flashcardService.GetFlashcardsByCategoryAsync(category, cancellationToken);
            return Ok(flashcards);
        }

        /// <summary>
        /// Пошук флешкарток
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<FlashcardDto>>> SearchFlashcards([FromQuery] string searchTerm, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching flashcards with term: {SearchTerm}", searchTerm);
            var flashcards = await _flashcardService.SearchFlashcardsAsync(searchTerm, cancellationToken);
            return Ok(flashcards);
        }

        /// <summary>
        /// Створити нову флешкартку
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateFlashcard([FromBody] CreateFlashcardDto createFlashcardDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new flashcard for user: {UserId}", createFlashcardDto.UserId);
            var flashcardId = await _flashcardService.CreateFlashcardAsync(createFlashcardDto, cancellationToken);

            return CreatedAtAction(nameof(GetFlashcardById), new { id = flashcardId }, flashcardId);
        }

        /// <summary>
        /// Оновити флешкартку
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFlashcard(int id, [FromBody] UpdateFlashcardDto updateFlashcardDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating flashcard with ID: {FlashcardId}", id);
            var result = await _flashcardService.UpdateFlashcardAsync(id, updateFlashcardDto, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Видалити флешкартку
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFlashcard(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting flashcard with ID: {FlashcardId}", id);
            var result = await _flashcardService.DeleteFlashcardAsync(id, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Отримати всі категорії
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetAllCategories(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all categories");
            var categories = await _flashcardService.GetAllCategoriesAsync(cancellationToken);
            return Ok(categories);
        }
    }
}
