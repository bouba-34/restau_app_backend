using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Reservation
{
    public class ReservationSummaryDto
    {
        public string Id { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public string TableNumber { get; set; }
        public ReservationStatus Status { get; set; }
        public string CustomerName { get; set; }
    }
}