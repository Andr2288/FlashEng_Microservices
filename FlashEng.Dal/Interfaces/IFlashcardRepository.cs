using FlashEng.Domain.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Interfaces
{
    public interface IFlashcardRepository
    {
        Task<List<Flashcard>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default);
        Task<List<Flashcard>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Flashcard?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<int> CreateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default);
        Task<bool> UpdateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default);
        Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    }
}
