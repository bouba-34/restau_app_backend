using Microsoft.EntityFrameworkCore;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Implementations
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;

            _context.Notifications.Update(notification);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Count == 0)
                return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            _context.Notifications.UpdateRange(unreadNotifications);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Notification> GetByIdAsync(string id)
        {
            return await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}