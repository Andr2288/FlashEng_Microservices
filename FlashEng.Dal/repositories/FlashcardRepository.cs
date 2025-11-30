using Dapper;
using FlashEng.Dal.Context;
using FlashEng.Dal.Interfaces;
using FlashEng.Dal.repositories;
using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
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
    public class FlashcardRepository : GenericRepository<Flashcard>, IFlashcardRepository
    {
        private readonly AppDbContext _context;

        public FlashcardRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Flashcard>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .OrderBy(f => f.Category)
                .ThenBy(f => f.EnglishWord)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Flashcard>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Category)
                .ThenBy(f => f.EnglishWord)
                .ToListAsync(cancellationToken);
        }

        public async Task<Flashcard?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .FirstOrDefaultAsync(f => f.FlashcardId == flashcardId, cancellationToken);
        }

        public async Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .Where(f => f.Category == category)
                .OrderBy(f => f.EnglishWord)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .Where(f => f.EnglishWord.Contains(searchTerm) ||
                           f.Translation.Contains(searchTerm) ||
                           f.Category.Contains(searchTerm))
                .OrderBy(f => f.EnglishWord)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CreateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            flashcard.CreatedAt = DateTime.UtcNow;
            flashcard.UpdatedAt = DateTime.UtcNow;

            await _context.Flashcards.AddAsync(flashcard, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return flashcard.FlashcardId;
        }

        public async Task<bool> UpdateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            flashcard.UpdatedAt = DateTime.UtcNow;
            _context.Flashcards.Update(flashcard);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            var flashcard = await GetFlashcardByIdAsync(flashcardId, cancellationToken);
            if (flashcard == null) return false;

            _context.Flashcards.Remove(flashcard);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Flashcards
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(cancellationToken);
        }
    }
}
