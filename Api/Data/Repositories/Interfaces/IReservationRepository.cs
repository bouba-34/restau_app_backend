using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<bool> UpdateReservationStatusAsync(string reservationId, ReservationStatus status);
        Task<List<Table>> GetAllTablesAsync();
        Task<Table> GetTableByNumberAsync(string tableNumber);
        Task<List<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int partySize);
        Task<bool> IsTableAvailableAsync(string tableNumber, DateTime date, TimeSpan time);
    }
}