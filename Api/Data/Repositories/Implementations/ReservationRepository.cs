using Microsoft.EntityFrameworkCore;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Implementations
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> UpdateReservationStatusAsync(string reservationId, ReservationStatus status)
        {
            var reservation = await _dbSet.FindAsync(reservationId);
            if (reservation == null)
                return false;

            reservation.Status = status;
            reservation.UpdatedAt = DateTime.UtcNow;

            _context.Reservations.Update(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Table>> GetAllTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive)
                .OrderBy(t => t.Number)
                .ToListAsync();
        }

        public async Task<Table> GetTableByNumberAsync(string tableNumber)
        {
            return await _context.Tables
                .FirstOrDefaultAsync(t => t.Number == tableNumber);
        }

        public async Task<List<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int partySize)
        {
            // Get all active tables with enough capacity
            var tables = await _context.Tables
                .Where(t => t.IsActive && t.Capacity >= partySize)
                .ToListAsync();

            var availableTables = new List<Table>();
            
            // Check each table for availability
            foreach (var table in tables)
            {
                if (await IsTableAvailableAsync(table.Number, date, time))
                {
                    availableTables.Add(table);
                }
            }
            
            // Return tables ordered by capacity (to assign most efficient table)
            return availableTables.OrderBy(t => t.Capacity).ToList();
        }

        public async Task<bool> IsTableAvailableAsync(string tableNumber, DateTime date, TimeSpan time)
        {
            // Check if table exists and is active
            var table = await GetTableByNumberAsync(tableNumber);
            if (table == null || !table.IsActive)
                return false;
            
            // Define reservation time window (typically 2 hours)
            var startWindow = time.Add(TimeSpan.FromHours(-1));
            var endWindow = time.Add(TimeSpan.FromHours(1));
            
            // Check if there are any overlapping reservations
            var conflictingReservations = await _context.Reservations
                .Where(r => r.TableNumber == tableNumber &&
                           r.ReservationDate.Date == date.Date &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.NoShow &&
                           ((r.ReservationTime >= startWindow && r.ReservationTime <= endWindow) ||
                            (r.ReservationTime.Add(TimeSpan.FromHours(1.5)) >= startWindow && 
                             r.ReservationTime.Add(TimeSpan.FromHours(1.5)) <= endWindow)))
                .AnyAsync();
                
            return !conflictingReservations;
        }

        public override async Task<Reservation> GetByIdAsync(string id)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();
        }
    }
}