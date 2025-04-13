using backend.Api.Controllers;
using RestaurantManagement.Api.Controllers;
using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetUsersByTypeAsync(UserType userType);
        Task<string> CreateUserAsync(CreateUserDto userDto);
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto userDto);
        Task<bool> UpdateUserStatusAsync(string userId, bool isActive);
        Task<bool> DeleteUserAsync(string userId);
    }
}