using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Menu
{
    public class UpdateMenuItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 1000)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsVegetarian { get; set; }

        public bool IsVegan { get; set; }

        public bool IsGlutenFree { get; set; }

        [Range(1, 120)]
        public int PreparationTimeMinutes { get; set; }

        [Required]
        public string CategoryId { get; set; }

        public List<string> Ingredients { get; set; } = new List<string>();

        public List<string> Allergens { get; set; } = new List<string>();

        [Range(0, 5000)]
        public int Calories { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }
    }
}