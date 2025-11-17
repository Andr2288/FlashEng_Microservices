using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Models;
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
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly string _connectionString;

        public FlashcardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Flashcard>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default)
        {
            var flashcards = new List<Flashcard>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Flashcards ORDER BY Category, EnglishWord";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                flashcards.Add(new Flashcard
                {
                    FlashcardId = reader.GetInt32("FlashcardId"),
                    UserId = reader.GetInt32("UserId"),
                    Category = reader.GetString("Category"),
                    EnglishWord = reader.GetString("EnglishWord"),
                    Translation = reader.GetString("Translation"),
                    Definition = reader.IsDBNull("Definition") ? null : reader.GetString("Definition"),
                    ExampleSentence = reader.IsDBNull("ExampleSentence") ? null : reader.GetString("ExampleSentence"),
                    Pronunciation = reader.IsDBNull("Pronunciation") ? null : reader.GetString("Pronunciation"),
                    AudioUrl = reader.IsDBNull("AudioUrl") ? null : reader.GetString("AudioUrl"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Difficulty = reader.GetString("Difficulty"),
                    IsPublic = reader.GetBoolean("IsPublic"),
                    Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                });
            }

            return flashcards;
        }

        public async Task<List<Flashcard>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var flashcards = new List<Flashcard>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Flashcards WHERE UserId = @UserId ORDER BY Category, EnglishWord";
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                flashcards.Add(new Flashcard
                {
                    FlashcardId = reader.GetInt32("FlashcardId"),
                    UserId = reader.GetInt32("UserId"),
                    Category = reader.GetString("Category"),
                    EnglishWord = reader.GetString("EnglishWord"),
                    Translation = reader.GetString("Translation"),
                    Definition = reader.IsDBNull("Definition") ? null : reader.GetString("Definition"),
                    ExampleSentence = reader.IsDBNull("ExampleSentence") ? null : reader.GetString("ExampleSentence"),
                    Pronunciation = reader.IsDBNull("Pronunciation") ? null : reader.GetString("Pronunciation"),
                    AudioUrl = reader.IsDBNull("AudioUrl") ? null : reader.GetString("AudioUrl"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Difficulty = reader.GetString("Difficulty"),
                    IsPublic = reader.GetBoolean("IsPublic"),
                    Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                });
            }

            return flashcards;
        }

        public async Task<Flashcard?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Flashcards WHERE FlashcardId = @FlashcardId";
            command.Parameters.AddWithValue("@FlashcardId", flashcardId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new Flashcard
                {
                    FlashcardId = reader.GetInt32("FlashcardId"),
                    UserId = reader.GetInt32("UserId"),
                    Category = reader.GetString("Category"),
                    EnglishWord = reader.GetString("EnglishWord"),
                    Translation = reader.GetString("Translation"),
                    Definition = reader.IsDBNull("Definition") ? null : reader.GetString("Definition"),
                    ExampleSentence = reader.IsDBNull("ExampleSentence") ? null : reader.GetString("ExampleSentence"),
                    Pronunciation = reader.IsDBNull("Pronunciation") ? null : reader.GetString("Pronunciation"),
                    AudioUrl = reader.IsDBNull("AudioUrl") ? null : reader.GetString("AudioUrl"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Difficulty = reader.GetString("Difficulty"),
                    IsPublic = reader.GetBoolean("IsPublic"),
                    Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                };
            }

            return null;
        }

        public async Task<List<Flashcard>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            var flashcards = new List<Flashcard>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Flashcards WHERE Category = @Category ORDER BY EnglishWord";
            command.Parameters.AddWithValue("@Category", category);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                flashcards.Add(new Flashcard
                {
                    FlashcardId = reader.GetInt32("FlashcardId"),
                    UserId = reader.GetInt32("UserId"),
                    Category = reader.GetString("Category"),
                    EnglishWord = reader.GetString("EnglishWord"),
                    Translation = reader.GetString("Translation"),
                    Definition = reader.IsDBNull("Definition") ? null : reader.GetString("Definition"),
                    ExampleSentence = reader.IsDBNull("ExampleSentence") ? null : reader.GetString("ExampleSentence"),
                    Pronunciation = reader.IsDBNull("Pronunciation") ? null : reader.GetString("Pronunciation"),
                    AudioUrl = reader.IsDBNull("AudioUrl") ? null : reader.GetString("AudioUrl"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Difficulty = reader.GetString("Difficulty"),
                    IsPublic = reader.GetBoolean("IsPublic"),
                    Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                });
            }

            return flashcards;
        }

        public async Task<List<Flashcard>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var flashcards = new List<Flashcard>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT * FROM Flashcards 
            WHERE EnglishWord LIKE @SearchTerm 
               OR Translation LIKE @SearchTerm 
               OR Category LIKE @SearchTerm
            ORDER BY EnglishWord";

            var searchPattern = $"%{searchTerm}%";
            command.Parameters.AddWithValue("@SearchTerm", searchPattern);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                flashcards.Add(new Flashcard
                {
                    FlashcardId = reader.GetInt32("FlashcardId"),
                    UserId = reader.GetInt32("UserId"),
                    Category = reader.GetString("Category"),
                    EnglishWord = reader.GetString("EnglishWord"),
                    Translation = reader.GetString("Translation"),
                    Definition = reader.IsDBNull("Definition") ? null : reader.GetString("Definition"),
                    ExampleSentence = reader.IsDBNull("ExampleSentence") ? null : reader.GetString("ExampleSentence"),
                    Pronunciation = reader.IsDBNull("Pronunciation") ? null : reader.GetString("Pronunciation"),
                    AudioUrl = reader.IsDBNull("AudioUrl") ? null : reader.GetString("AudioUrl"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Difficulty = reader.GetString("Difficulty"),
                    IsPublic = reader.GetBoolean("IsPublic"),
                    Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                });
            }

            return flashcards;
        }

        public async Task<int> CreateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Flashcards (UserId, Category, EnglishWord, Translation, Definition, 
                                   ExampleSentence, Pronunciation, AudioUrl, ImageUrl, 
                                   Difficulty, IsPublic, Price)
            VALUES (@UserId, @Category, @EnglishWord, @Translation, @Definition, 
                    @ExampleSentence, @Pronunciation, @AudioUrl, @ImageUrl, 
                    @Difficulty, @IsPublic, @Price);
            SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@UserId", flashcard.UserId);
            command.Parameters.AddWithValue("@Category", flashcard.Category);
            command.Parameters.AddWithValue("@EnglishWord", flashcard.EnglishWord);
            command.Parameters.AddWithValue("@Translation", flashcard.Translation);
            command.Parameters.AddWithValue("@Definition", flashcard.Definition ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ExampleSentence", flashcard.ExampleSentence ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pronunciation", flashcard.Pronunciation ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AudioUrl", flashcard.AudioUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ImageUrl", flashcard.ImageUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Difficulty", flashcard.Difficulty);
            command.Parameters.AddWithValue("@IsPublic", flashcard.IsPublic);
            command.Parameters.AddWithValue("@Price", flashcard.Price ?? (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateFlashcardAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE Flashcards 
            SET Category = @Category, EnglishWord = @EnglishWord, Translation = @Translation,
                Definition = @Definition, ExampleSentence = @ExampleSentence, 
                Pronunciation = @Pronunciation, AudioUrl = @AudioUrl, ImageUrl = @ImageUrl,
                Difficulty = @Difficulty, IsPublic = @IsPublic, Price = @Price,
                UpdatedAt = NOW()
            WHERE FlashcardId = @FlashcardId";

            command.Parameters.AddWithValue("@FlashcardId", flashcard.FlashcardId);
            command.Parameters.AddWithValue("@Category", flashcard.Category);
            command.Parameters.AddWithValue("@EnglishWord", flashcard.EnglishWord);
            command.Parameters.AddWithValue("@Translation", flashcard.Translation);
            command.Parameters.AddWithValue("@Definition", flashcard.Definition ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ExampleSentence", flashcard.ExampleSentence ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pronunciation", flashcard.Pronunciation ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AudioUrl", flashcard.AudioUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ImageUrl", flashcard.ImageUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Difficulty", flashcard.Difficulty);
            command.Parameters.AddWithValue("@IsPublic", flashcard.IsPublic);
            command.Parameters.AddWithValue("@Price", flashcard.Price ?? (object)DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Flashcards WHERE FlashcardId = @FlashcardId";
            command.Parameters.AddWithValue("@FlashcardId", flashcardId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var categories = new List<string>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT Category FROM Flashcards ORDER BY Category";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                categories.Add(reader.GetString("Category"));
            }

            return categories;
        }
    }
}
