using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));
                
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(ApiResponse<List<Notification>>.SuccessResponse(notifications));
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));
                
            var notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return Ok(ApiResponse<List<Notification>>.SuccessResponse(notifications));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(string id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            
            if (notification == null)
                return NotFound(new ErrorResponse("Notification not found"));
                
            // Check if the notification belongs to the current user
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole != "Admin" && notification.UserId != userId)
                return Forbid();
                
            return Ok(ApiResponse<Notification>.SuccessResponse(notification));
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            // First check if notification exists and belongs to user
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            
            if (notification == null)
                return NotFound(new ErrorResponse("Notification not found"));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole != "Admin" && notification.UserId != userId)
                return Forbid();
            
            var success = await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification marked as read"));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));
                
            var success = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return Ok(ApiResponse<bool>.SuccessResponse(success, "All notifications marked as read"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            // First check if notification exists and belongs to user
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            
            if (notification == null)
                return NotFound(new ErrorResponse("Notification not found"));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole != "Admin" && notification.UserId != userId)
                return Forbid();
            
            var success = await _notificationService.DeleteNotificationAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification deleted"));
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var success = await _notificationService.SendPromotionalNotificationAsync(
                request.Title, 
                request.Message, 
                request.UserIds);
                
            return Ok(ApiResponse<bool>.SuccessResponse(success, "Notification broadcast sent"));
        }

        [HttpPost("system")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendSystemNotification([FromBody] SystemNotificationDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new ErrorResponse("User ID is required"));
                
            var success = await _notificationService.SendSystemNotificationAsync(
                request.UserId,
                request.Title,
                request.Message);
                
            return Ok(ApiResponse<bool>.SuccessResponse(success, "System notification sent"));
        }
    }

    public class BroadcastNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public List<string> UserIds { get; set; } // Optional: if null, send to all users
    }

    public class SystemNotificationDto
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}