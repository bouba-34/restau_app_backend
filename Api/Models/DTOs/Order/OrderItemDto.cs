using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Order
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string MenuItemImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public List<string> Customizations { get; set; } = new List<string>();
        public string SpecialInstructions { get; set; }
    }
}