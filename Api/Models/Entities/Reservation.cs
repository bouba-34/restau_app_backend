namespace backend.Api.Models.Entities
{
    public class Reservation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; }
        public User Customer { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public string TableNumber { get; set; }
        public ReservationStatus Status { get; set; }
        public string SpecialRequests { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled,
        NoShow
    }
}