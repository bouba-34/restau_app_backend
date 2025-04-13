using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using backend.Api.Models.Entities;

namespace backend.Api.Hubs
{
    [Authorize]
    public class RestaurantHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;

                // Add user to role-based group
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, userRole);
                }

                await Clients.Caller.SendAsync("Connected", $"Connected with ID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId) && UserConnections.ContainsKey(userId))
            {
                UserConnections.Remove(userId);

                // Remove user from role-based group
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userRole);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Order notification methods
        public async Task NotifyOrderStatusChanged(string orderId, OrderStatus status, string customerId)
        {
            // Notify customer
            if (UserConnections.ContainsKey(customerId))
            {
                await Clients.Client(UserConnections[customerId]).SendAsync("OrderStatusChanged", orderId, status);
            }

            // Notify staff and admins
            await Clients.Group(UserType.Staff.ToString()).SendAsync("OrderStatusChanged", orderId, status);
            await Clients.Group(UserType.Admin.ToString()).SendAsync("OrderStatusChanged", orderId, status);
        }

        public async Task NotifyNewOrder(string orderId)
        {
            // Notify staff and admins
            await Clients.Group(UserType.Staff.ToString()).SendAsync("NewOrder", orderId);
            await Clients.Group(UserType.Admin.ToString()).SendAsync("NewOrder", orderId);
        }

        // Reservation notification methods
        public async Task NotifyReservationStatusChanged(string reservationId, ReservationStatus status, string customerId)
        {
            // Notify customer
            if (UserConnections.ContainsKey(customerId))
            {
                await Clients.Client(UserConnections[customerId]).SendAsync("ReservationStatusChanged", reservationId, status);
            }

            // Notify staff and admins
            await Clients.Group(UserType.Staff.ToString()).SendAsync("ReservationStatusChanged", reservationId, status);
            await Clients.Group(UserType.Admin.ToString()).SendAsync("ReservationStatusChanged", reservationId, status);
        }

        public async Task NotifyNewReservation(string reservationId)
        {
            // Notify staff and admins
            await Clients.Group(UserType.Staff.ToString()).SendAsync("NewReservation", reservationId);
            await Clients.Group(UserType.Admin.ToString()).SendAsync("NewReservation", reservationId);
        }

        // Menu updates
        public async Task NotifyMenuItemAvailabilityChanged(string menuItemId, bool isAvailable)
        {
            // Notify all connected clients
            await Clients.All.SendAsync("MenuItemAvailabilityChanged", menuItemId, isAvailable);
        }

        // Custom notifications
        public async Task SendNotificationToUser(string userId, string title, string message)
        {
            if (UserConnections.ContainsKey(userId))
            {
                await Clients.Client(UserConnections[userId]).SendAsync("Notification", title, message);
            }
        }

        public async Task SendNotificationToAllCustomers(string title, string message)
        {
            await Clients.Group(UserType.Customer.ToString()).SendAsync("Notification", title, message);
        }

        public async Task SendNotificationToStaff(string title, string message)
        {
            await Clients.Group(UserType.Staff.ToString()).SendAsync("Notification", title, message);
            await Clients.Group(UserType.Admin.ToString()).SendAsync("Notification", title, message);
        }
    }
}