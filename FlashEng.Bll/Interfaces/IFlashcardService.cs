using FlashEng.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Interfaces
{
    public interface IFlashcardService
    {
        Task<List<FlashcardDto>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default);
        Task<FlashcardDto?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<FlashcardDto>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<int> CreateFlashcardAsync(CreateFlashcardDto createFlashcardDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateFlashcardAsync(int flashcardId, UpdateFlashcardDto updateFlashcardDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    }
}
