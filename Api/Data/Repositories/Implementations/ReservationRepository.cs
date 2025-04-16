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

            var reservationDuration = TimeSpan.FromMinutes(90);
            var requestedStart = time;
            var requestedEnd = time + reservationDuration;

            // On récupère toutes les réservations concernées côté base
            var reservations = await _context.Reservations
                .Where(r => r.TableNumber == tableNumber &&
                            r.ReservationDate.Date == date.Date &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.Status != ReservationStatus.NoShow)
                .ToListAsync(); // passage en mémoire ici

            // Vérifie le chevauchement en mémoire
            foreach (var reservation in reservations)
            {
                var existingStart = reservation.ReservationTime;
                var existingEnd = existingStart + reservationDuration;

                // Chevauchement ?
                if (existingStart < requestedEnd && existingEnd > requestedStart)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Reservation> GetByIdAsync(string id)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();
        }
    }
}