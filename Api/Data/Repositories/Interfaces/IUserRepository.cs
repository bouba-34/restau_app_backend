using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> UpdateUserPasswordAsync(string userId, string passwordHash);
        Task<bool> UpdateUserStatusAsync(string userId, bool isActive);
    }
}