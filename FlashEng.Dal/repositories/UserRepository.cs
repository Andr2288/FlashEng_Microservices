using Dapper;
using FlashEng.Dal.Context;
using FlashEng.Dal.Interfaces;
using FlashEng.Dal.repositories;
using FlashEng.Domain.Models;
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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.CreatedAt = DateTime.UtcNow;
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user.UserId;
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            _context.Users.Remove(user);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<UserSettings?> GetUserSettingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);
        }

        public async Task<int> CreateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            await _context.UserSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return settings.SettingsId;
        }

        public async Task<bool> UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            _context.UserSettings.Update(settings);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }
}
