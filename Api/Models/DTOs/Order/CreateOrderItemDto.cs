using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public string MenuItemId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        public List<string> Customizations { get; set; } = new List<string>();

        public string SpecialInstructions { get; set; } = "None";
    }
}