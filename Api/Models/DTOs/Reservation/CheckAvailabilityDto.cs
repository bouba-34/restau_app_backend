using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Reservation
{
    public class CheckAvailabilityDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [Required]
        [Range(1, 20)]
        public int PartySize { get; set; }
    }
}