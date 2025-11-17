using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Dal.Repositories
{
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly string _connectionString;

        public FlashcardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Flashcard>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Flashcards ORDER BY Category, EnglishWord";
            var flashcards = await connection.QueryAsync<Flashcard>(sql);
            return flashcards.ToList();
        }

        public async Task<List<Flashcard>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Flashcards WHERE UserId = @UserId ORDER BY Category, EnglishWord";
            var flashcards = await connection.QueryAsync<Flashcard>(sql, new { UserId = userId });
            return flashcards.ToList();
        }

        public async Task<Flashcard?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Flashcards WHERE FlashcardId = @FlashcardId";
            return await connection.QueryFirstOrDefaultAsync<Flashcard>(sql, new { FlashcardId = flashcardId });
        }

        public async Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT * FROM Flashcards WHERE Category = @Category ORDER BY EnglishWord";
            var flashcards = await connection.QueryAsync<Flashcard>(sql, new { Category = category });
            return flashcards.ToList();
        }

        public async Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            SELECT * FROM Flashcards 
            WHERE EnglishWord LIKE @SearchTerm 
               OR Translation LIKE @SearchTerm 
               OR Category LIKE @SearchTerm
            ORDER BY EnglishWord";

            var searchPattern = $"%{searchTerm}%";
            var flashcards = await connection.QueryAsync<Flashcard>(sql, new { SearchTerm = searchPattern });
            return flashcards.ToList();
        }

        public async Task<int> CreateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            INSERT INTO Flashcards (UserId, Category, EnglishWord, Translation, Definition, 
                                   ExampleSentence, Pronunciation, AudioUrl, ImageUrl, 
                                   Difficulty, IsPublic, Price)
            VALUES (@UserId, @Category, @EnglishWord, @Translation, @Definition, 
                    @ExampleSentence, @Pronunciation, @AudioUrl, @ImageUrl, 
                    @Difficulty, @IsPublic, @Price);
            SELECT LAST_INSERT_ID();";

            return await connection.QuerySingleAsync<int>(sql, flashcard);
        }

        public async Task<bool> UpdateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = @"
            UPDATE Flashcards 
            SET Category = @Category, EnglishWord = @EnglishWord, Translation = @Translation,
                Definition = @Definition, ExampleSentence = @ExampleSentence, 
                Pronunciation = @Pronunciation, AudioUrl = @AudioUrl, ImageUrl = @ImageUrl,
                Difficulty = @Difficulty, IsPublic = @IsPublic, Price = @Price,
                UpdatedAt = NOW()
            WHERE FlashcardId = @FlashcardId";

            var rowsAffected = await connection.ExecuteAsync(sql, flashcard);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "DELETE FROM Flashcards WHERE FlashcardId = @FlashcardId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { FlashcardId = flashcardId });
            return rowsAffected > 0;
        }

        public async Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            var sql = "SELECT DISTINCT Category FROM Flashcards ORDER BY Category";
            var categories = await connection.QueryAsync<string>(sql);
            return categories.ToList();
        }
    }
}
