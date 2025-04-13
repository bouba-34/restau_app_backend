namespace backend.Api.Models.Entities
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; }
        public User Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public OrderStatus Status { get; set; }
        public OrderType Type { get; set; }
        public string TableNumber { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TipAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string SpecialInstructions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PreparedById { get; set; }
        public User PreparedBy { get; set; }
        public int EstimatedWaitTimeMinutes { get; set; }
    }

    public enum OrderStatus
    {
        Placed,
        Preparing,
        Ready,
        Served,
        Completed,
        Cancelled
    }

    public enum OrderType
    {
        DineIn,
        TakeOut,
        Delivery
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }
}