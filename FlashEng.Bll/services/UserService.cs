using AutoMapper;
using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Exceptions;
using FlashEng.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Bll.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _unitOfWork.Users.GetAllUsersAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId, cancellationToken);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email cannot be empty");

            var user = await _unitOfWork.Users.GetUserByEmailAsync(email, cancellationToken);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<int> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(createUserDto.Email))
                throw new ValidationException("Email is required");

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
                throw new ValidationException("Password is required");

            if (string.IsNullOrWhiteSpace(createUserDto.FullName))
                throw new ValidationException("Full name is required");

            // Перевірка на існуючого користувача
            var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(createUserDto.Email, cancellationToken);
            if (existingUser != null)
                throw new BusinessConflictException("User with this email already exists");

            var user = _mapper.Map<User>(createUserDto);
            var userId = await _unitOfWork.Users.CreateUserAsync(user, cancellationToken);

            // Створюємо налаштування за замовчуванням
            var settings = new UserSettings
            {
                UserId = userId,
                Theme = "Light",
                Language = "en",
                NotificationsEnabled = true
            };

            await _unitOfWork.Users.CreateUserSettingsAsync(settings, cancellationToken);

            return userId;
        }

        public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var existingUser = await _unitOfWork.Users.GetUserByIdAsync(userId, cancellationToken);
            if (existingUser == null)
                throw new NotFoundException("User", userId);

            if (string.IsNullOrWhiteSpace(updateUserDto.FullName))
                throw new ValidationException("Full name is required");

            _mapper.Map(updateUserDto, existingUser);

            return await _unitOfWork.Users.UpdateUserAsync(existingUser, cancellationToken);
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var existingUser = await _unitOfWork.Users.GetUserByIdAsync(userId, cancellationToken);
            if (existingUser == null)
                throw new NotFoundException("User", userId);

            return await _unitOfWork.Users.DeleteUserAsync(userId, cancellationToken);
        }

        public async Task<UserSettingsDto?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var settings = await _unitOfWork.Users.GetUserSettingsAsync(userId, cancellationToken);
            return settings != null ? _mapper.Map<UserSettingsDto>(settings) : null;
        }

        public async Task<bool> UpdateUserSettingsAsync(int userId, UserSettingsDto settingsDto, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var existingSettings = await _unitOfWork.Users.GetUserSettingsAsync(userId, cancellationToken);
            if (existingSettings == null)
                throw new NotFoundException("UserSettings", userId);

            var settings = _mapper.Map<UserSettings>(settingsDto);
            settings.UserId = userId; // Переконуємося що ID правильний

            return await _unitOfWork.Users.UpdateUserSettingsAsync(settings, cancellationToken);
        }
    }
}
