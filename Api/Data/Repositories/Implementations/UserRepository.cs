using Microsoft.EntityFrameworkCore;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> UpdateUserPasswordAsync(string userId, string passwordHash)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = passwordHash;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}