using FlashEng.Domain.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserSettings?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> CreateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
    }
}
