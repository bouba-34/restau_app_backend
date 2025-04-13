using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Auth;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Regular users can only access their own profile
            if (userRole != "Admin" && userId != id)
                return Forbid();
                
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return NotFound(new ErrorResponse("User not found"));
                
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            
            if (user == null)
                return NotFound(new ErrorResponse("User not found"));
                
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
                return NotFound(new ErrorResponse("User not found"));
                
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet("by-type/{userType}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByType(UserType userType)
        {
            var users = await _userService.GetUsersByTypeAsync(userType);
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = await _userService.CreateUserAsync(userDto);
            
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorResponse("Username or email already exists"));
                
            var user = await _userService.GetUserByIdAsync(userId);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, ApiResponse<UserDto>.SuccessResponse(user, "User created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Regular users can only update their own profile
            if (userRole != "Admin" && userId != id)
                return Forbid();
                
            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            
            if (updatedUser == null)
                return NotFound(new ErrorResponse("User not found or update failed"));
                
            return Ok(ApiResponse<UserDto>.SuccessResponse(updatedUser, "User updated successfully"));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDto statusDto)
        {
            var success = await _userService.UpdateUserStatusAsync(id, statusDto.IsActive);
            
            if (!success)
                return NotFound(new ErrorResponse("User not found or status update failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "User status updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            
            if (!success)
                return NotFound(new ErrorResponse("User not found or deletion failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
        }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}