using FlashEng.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<int> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(int userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<UserSettingsDto?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserSettingsAsync(int userId, UserSettingsDto settingsDto, CancellationToken cancellationToken = default);
    }
}
