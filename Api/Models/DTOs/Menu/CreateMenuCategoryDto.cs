using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Menu
{
    public class CreateMenuCategoryDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}