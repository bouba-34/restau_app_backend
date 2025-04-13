using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Order
{
    public class OrderDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
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
        public string PreparedByName { get; set; }
        public int EstimatedWaitTimeMinutes { get; set; }
    }
}