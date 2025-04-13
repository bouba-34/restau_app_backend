namespace backend.Api.Models.Entities
{
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
    }

    public enum NotificationType
    {
        OrderStatus,
        Reservation,
        Promotion,
        System
    }
}