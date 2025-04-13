using AutoMapper;
using RestaurantManagement.Api.Controllers;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using backend.Api.Controllers;
using backend.Api.Repositories.Interfaces;

namespace backend.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetUsersByTypeAsync(UserType userType)
        {
            var users = await _userRepository.GetAsync(u => u.UserType == userType);
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<string> CreateUserAsync(CreateUserDto userDto)
        {
            // Check if username or email already exists
            var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => 
                u.Username.ToLower() == userDto.Username.ToLower() ||
                u.Email.ToLower() == userDto.Email.ToLower());

            if (existingUser != null)
                return null;

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = userDto.Username,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                PasswordHash = HashPassword(userDto.Password),
                UserType = userDto.UserType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            return user.Id;
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            // Update user properties
            if (!string.IsNullOrEmpty(userDto.Email))
            {
                // Check if email is already taken by another user
                var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => 
                    u.Email.ToLower() == userDto.Email.ToLower() && u.Id != userId);

                if (existingUser != null)
                    return null;

                user.Email = userDto.Email;
            }

            if (!string.IsNullOrEmpty(userDto.PhoneNumber))
            {
                user.PhoneNumber = userDto.PhoneNumber;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _userRepository.RemoveAsync(userId);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}