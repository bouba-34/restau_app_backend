using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Auth;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var response = await _authService.LoginAsync(loginRequest);

            if (response == null)
                return Unauthorized(new ErrorResponse("Invalid username or password"));

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var response = await _authService.RegisterAsync(registerRequest);

            if (response == null)
                return BadRequest(new ErrorResponse("Username or email already exists"));

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Registration successful"));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));

            var result = await _authService.ChangePasswordAsync(userId, changePasswordRequest);

            if (!result)
                return BadRequest(new ErrorResponse("Current password is incorrect"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully"));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new ErrorResponse("User not found"));

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.PhoneNumber,
                user.UserType,
                user.CreatedAt
            }));
        }
    }
}