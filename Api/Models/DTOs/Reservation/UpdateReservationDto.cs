using System.ComponentModel.DataAnnotations;

namespace backend.Api.Models.DTOs.Reservation
{
    public class UpdateReservationDto
    {
        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan ReservationTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int PartySize { get; set; }

        public string TableNumber { get; set; }

        public string SpecialRequests { get; set; }

        [Phone]
        public string ContactPhone { get; set; }

        [EmailAddress]
        public string ContactEmail { get; set; }
    }
}