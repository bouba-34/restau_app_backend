using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId);
        Task<Notification> GetNotificationByIdAsync(string notificationId);
        Task<string> CreateNotificationAsync(Notification notification);
        Task<bool> MarkNotificationAsReadAsync(string notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId);
        Task<bool> SendOrderStatusNotificationAsync(string orderId, OrderStatus status);
        Task<bool> SendReservationStatusNotificationAsync(string reservationId, ReservationStatus status);
        Task<bool> SendSystemNotificationAsync(string userId, string title, string message);
        Task<bool> SendPromotionalNotificationAsync(string title, string message, List<string> userIds = null);
    }
}