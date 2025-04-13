using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Menu
{
    public class MenuItemAvailabilityDto
    {
        [Required]
        public bool IsAvailable { get; set; }
    }
}