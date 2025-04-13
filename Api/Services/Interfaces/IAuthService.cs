using backend.Api.Models.DTOs.Auth;
using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
        Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest changePasswordRequest);
        Task<User> GetUserByIdAsync(string userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<bool> ValidateUserAsync(string username, string password);
    }
}