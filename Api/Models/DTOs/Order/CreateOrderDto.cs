using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();

        [Required]
        public OrderType Type { get; set; }

        public string TableNumber { get; set; }

        public string SpecialInstructions { get; set; }

        public string PaymentMethod { get; set; }

        public decimal TipAmount { get; set; }
    }
}