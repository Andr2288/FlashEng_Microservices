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
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = new List<User>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Email, PasswordHash, FullName, Role, IsActive, CreatedAt FROM UserProfiles ORDER BY CreatedAt DESC";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    FullName = reader.GetString("FullName"),
                    Role = reader.GetString("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Email, PasswordHash, FullName, Role, IsActive, CreatedAt FROM UserProfiles WHERE UserId = @UserId";
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    FullName = reader.GetString("FullName"),
                    Role = reader.GetString("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Email, PasswordHash, FullName, Role, IsActive, CreatedAt FROM UserProfiles WHERE Email = @Email";
            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    FullName = reader.GetString("FullName"),
                    Role = reader.GetString("Role"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        public async Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO UserProfiles (Email, PasswordHash, FullName, Role, IsActive)
            VALUES (@Email, @PasswordHash, @FullName, @Role, @IsActive);
            SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE UserProfiles 
            SET Email = @Email, PasswordHash = @PasswordHash, FullName = @FullName, 
                Role = @Role, IsActive = @IsActive
            WHERE UserId = @UserId";

            command.Parameters.AddWithValue("@UserId", user.UserId);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM UserProfiles WHERE UserId = @UserId";
            command.Parameters.AddWithValue("@UserId", userId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<UserSettings?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT SettingsId, UserId, Theme, Language, NotificationsEnabled FROM UserSettings WHERE UserId = @UserId";
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new UserSettings
                {
                    SettingsId = reader.GetInt32("SettingsId"),
                    UserId = reader.GetInt32("UserId"),
                    Theme = reader.GetString("Theme"),
                    Language = reader.GetString("Language"),
                    NotificationsEnabled = reader.GetBoolean("NotificationsEnabled")
                };
            }

            return null;
        }

        public async Task<int> CreateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO UserSettings (UserId, Theme, Language, NotificationsEnabled)
            VALUES (@UserId, @Theme, @Language, @NotificationsEnabled);
            SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@UserId", settings.UserId);
            command.Parameters.AddWithValue("@Theme", settings.Theme);
            command.Parameters.AddWithValue("@Language", settings.Language);
            command.Parameters.AddWithValue("@NotificationsEnabled", settings.NotificationsEnabled);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE UserSettings 
            SET Theme = @Theme, Language = @Language, NotificationsEnabled = @NotificationsEnabled
            WHERE UserId = @UserId";

            command.Parameters.AddWithValue("@UserId", settings.UserId);
            command.Parameters.AddWithValue("@Theme", settings.Theme);
            command.Parameters.AddWithValue("@Language", settings.Language);
            command.Parameters.AddWithValue("@NotificationsEnabled", settings.NotificationsEnabled);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
    }
}
