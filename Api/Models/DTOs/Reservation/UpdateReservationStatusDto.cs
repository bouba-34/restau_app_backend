using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Reservation
{
    public class UpdateReservationStatusDto
    {
        [Required]
        public ReservationStatus Status { get; set; }
    }
}