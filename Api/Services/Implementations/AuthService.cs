using Microsoft.IdentityModel.Tokens;
using backend.Api.Configuration;
using backend.Api.Models.DTOs.Auth;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Api.Data.Repositories.Interfaces;

namespace backend.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtConfig _jwtConfig;

        public AuthService(IUserRepository userRepository, JwtConfig jwtConfig)
        {
            _userRepository = userRepository;
            _jwtConfig = jwtConfig;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => 
                u.Username.ToLower() == loginRequest.Username.ToLower() && 
                u.IsActive);

            if (user == null || !VerifyPasswordHash(loginRequest.Password, user.PasswordHash))
                return null;

            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => 
                u.Username.ToLower() == registerRequest.Username.ToLower() ||
                u.Email.ToLower() == registerRequest.Email.ToLower());

            if (existingUser != null)
                return null;

            // Create new user
            var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                PasswordHash = HashPassword(registerRequest.Password),
                UserType = registerRequest.UserType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);

            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes)
            };
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest changePasswordRequest)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || !VerifyPasswordHash(changePasswordRequest.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(changePasswordRequest.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetFirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => 
                u.Username.ToLower() == username.ToLower() && 
                u.IsActive);

            if (user == null)
                return false;

            return VerifyPasswordHash(password, user.PasswordHash);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPasswordHash(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}