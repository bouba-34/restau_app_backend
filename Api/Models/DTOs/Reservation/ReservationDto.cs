using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Reservation
{
    public class ReservationDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public string TableNumber { get; set; }
        public ReservationStatus Status { get; set; }
        public string SpecialRequests { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}