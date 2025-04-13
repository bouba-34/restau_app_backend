using Microsoft.AspNetCore.SignalR;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Hubs;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;

namespace backend.Api.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IHubContext<RestaurantHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepository,
            IOrderRepository orderRepository,
            IReservationRepository reservationRepository,
            IHubContext<RestaurantHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _orderRepository = orderRepository;
            _reservationRepository = reservationRepository;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationsByUserIdAsync(userId);
        }

        public async Task<Notification> GetNotificationByIdAsync(string notificationId)
        {
            return await _notificationRepository.GetByIdAsync(notificationId);
        }

        public async Task<string> CreateNotificationAsync(Notification notification)
        {
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            await _notificationRepository.AddAsync(notification);

            // Send real-time notification
            await _hubContext.Clients.User(notification.UserId).SendAsync(
                "Notification", 
                notification.Title, 
                notification.Message
            );

            return notification.Id;
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
        {
            return await _notificationRepository.MarkNotificationAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            return await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            return await _notificationRepository.RemoveAsync(notification);
        }

        public async Task<bool> SendOrderStatusNotificationAsync(string orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Create message based on status
            string message = status switch
            {
                OrderStatus.Placed => "Your order has been received and is being processed.",
                OrderStatus.Preparing => "Your order is now being prepared in the kitchen.",
                OrderStatus.Ready => "Your order is ready for pickup or service.",
                OrderStatus.Served => "Your order has been served. Enjoy your meal!",
                OrderStatus.Completed => "Thank you for dining with us! Your order is now complete.",
                OrderStatus.Cancelled => "Your order has been cancelled.",
                _ => $"Your order status has been updated to {status}."
            };

            // Create notification
            var notification = new Notification
            {
                UserId = order.CustomerId,
                Title = $"Order #{order.Id.Substring(0, 8)} Update",
                Message = message,
                Type = NotificationType.OrderStatus,
                RelatedEntityId = orderId,
                RelatedEntityType = "Order"
            };

            await CreateNotificationAsync(notification);
            return true;
        }

        public async Task<bool> SendReservationStatusNotificationAsync(string reservationId, ReservationStatus status)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                return false;

            // Create message based on status
            string message = status switch
            {
                ReservationStatus.Pending => "Your reservation request has been received and is pending confirmation.",
                ReservationStatus.Confirmed => $"Your reservation for {reservation.ReservationDate.ToShortDateString()} at {reservation.ReservationTime} has been confirmed.",
                ReservationStatus.Completed => "Thank you for dining with us! Your reservation has been completed.",
                ReservationStatus.Cancelled => "Your reservation has been cancelled.",
                ReservationStatus.NoShow => "You were marked as a no-show for your reservation.",
                _ => $"Your reservation status has been updated to {status}."
            };

            // Create notification
            var notification = new Notification
            {
                UserId = reservation.CustomerId,
                Title = "Reservation Update",
                Message = message,
                Type = NotificationType.Reservation,
                RelatedEntityId = reservationId,
                RelatedEntityType = "Reservation"
            };

            await CreateNotificationAsync(notification);
            return true;
        }

        public async Task<bool> SendSystemNotificationAsync(string userId, string title, string message)
        {
            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.System
            };

            await CreateNotificationAsync(notification);
            return true;
        }

        public async Task<bool> SendPromotionalNotificationAsync(string title, string message, List<string> userIds = null)
        {
            if (userIds == null || !userIds.Any())
            {
                // If no specific users, broadcast to all connected clients with Customer role
                await _hubContext.Clients.Group("Customer").SendAsync("Notification", title, message);
                
                // We don't create persistent notifications here since we don't have user IDs
                return true;
            }
            
            // Create notifications for specific users
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = NotificationType.Promotion
                };

                await CreateNotificationAsync(notification);
            }

            return true;
        }
    }
}