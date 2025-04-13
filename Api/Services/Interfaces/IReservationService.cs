using backend.Api.Models.DTOs.Reservation;
using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> GetReservationByIdAsync(string reservationId);
        Task<List<ReservationDto>> GetReservationsByCustomerIdAsync(string customerId);
        Task<List<ReservationDto>> GetReservationsByDateAsync(DateTime date);
        Task<List<ReservationDto>> GetUpcomingReservationsAsync();
        Task<string> CreateReservationAsync(CreateReservationDto reservationDto, string customerId);
        Task<ReservationDto> UpdateReservationAsync(string reservationId, UpdateReservationDto reservationDto);
        Task<bool> UpdateReservationStatusAsync(string reservationId, ReservationStatus status);
        Task<bool> CancelReservationAsync(string reservationId);
        Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int partySize);
        Task<bool> IsTableAvailableAsync(string tableNumber, DateTime date, TimeSpan time);
    }
}