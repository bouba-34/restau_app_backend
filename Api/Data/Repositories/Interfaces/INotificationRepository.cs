using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(string notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
    }
}