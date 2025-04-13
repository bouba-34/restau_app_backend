using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Hubs;
using backend.Api.Models.DTOs.Reservation;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;

namespace backend.Api.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<RestaurantHub> _hubContext;
        private readonly IMapper _mapper;

        public ReservationService(
            IReservationRepository reservationRepository,
            IUserRepository userRepository,
            IHubContext<RestaurantHub> hubContext,
            IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<ReservationDto> GetReservationByIdAsync(string reservationId)
        {
            var reservation = await _reservationRepository.GetFirstOrDefaultAsync(
                r => r.Id == reservationId,
                "Customer"
            );

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<List<ReservationDto>> GetReservationsByCustomerIdAsync(string customerId)
        {
            var reservations = await _reservationRepository.GetAsync(
                r => r.CustomerId == customerId,
                query => query.OrderByDescending(r => r.ReservationDate).ThenBy(r => r.ReservationTime),
                "Customer"
            );

            return _mapper.Map<List<ReservationDto>>(reservations);
        }

        public async Task<List<ReservationDto>> GetReservationsByDateAsync(DateTime date)
        {
            var reservations = await _reservationRepository.GetAsync(
                r => r.ReservationDate.Date == date.Date,
                query => query.OrderBy(r => r.ReservationTime),
                "Customer"
            );

            return _mapper.Map<List<ReservationDto>>(reservations);
        }

        public async Task<List<ReservationDto>> GetUpcomingReservationsAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            var reservations = await _reservationRepository.GetAsync(
                r => (r.ReservationDate > today || 
                     (r.ReservationDate == today && r.ReservationTime > currentTime)) &&
                     r.Status != ReservationStatus.Cancelled && 
                     r.Status != ReservationStatus.NoShow,
                query => query.OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime),
                "Customer"
            );

            return _mapper.Map<List<ReservationDto>>(reservations);
        }

        public async Task<string> CreateReservationAsync(CreateReservationDto reservationDto, string customerId)
        {
            // Check if customer exists
            var customer = await _userRepository.GetByIdAsync(customerId);
            if (customer == null)
                return null;

            // Check if the reservation date and time is valid (not in the past)
            var reservationDateTime = reservationDto.ReservationDate.Date.Add(reservationDto.ReservationTime);
            if (reservationDateTime <= DateTime.Now)
                return null;

            // Find an available table
            var availableTables = await GetAvailableTablesAsync(
                reservationDto.ReservationDate,
                reservationDto.ReservationTime,
                reservationDto.PartySize
            );

            if (!availableTables.Any())
                return null;

            // Select first available table
            var selectedTable = availableTables.First();

            // Create reservation
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                Customer = customer,
                ReservationDate = reservationDto.ReservationDate,
                ReservationTime = reservationDto.ReservationTime,
                PartySize = reservationDto.PartySize,
                TableNumber = selectedTable.Number,
                Status = ReservationStatus.Pending,
                SpecialRequests = reservationDto.SpecialRequests,
                ContactPhone = reservationDto.ContactPhone ?? customer.PhoneNumber,
                ContactEmail = reservationDto.ContactEmail ?? customer.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save reservation
            await _reservationRepository.AddAsync(reservation);

            // Notify staff about new reservation
            await _hubContext.Clients.Groups(new[] { "Staff", "Admin" }).SendAsync("NewReservation", reservation.Id);

            return reservation.Id;
        }

        public async Task<ReservationDto> UpdateReservationAsync(string reservationId, UpdateReservationDto reservationDto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                return null;

            // Check if the table is available for the new date/time if it changed
            if (reservation.ReservationDate != reservationDto.ReservationDate || 
                reservation.ReservationTime != reservationDto.ReservationTime ||
                reservation.PartySize != reservationDto.PartySize)
            {
                // If table number is provided and different from current, check if it's available
                if (!string.IsNullOrEmpty(reservationDto.TableNumber) && 
                    reservationDto.TableNumber != reservation.TableNumber)
                {
                    var isAvailable = await IsTableAvailableAsync(
                        reservationDto.TableNumber,
                        reservationDto.ReservationDate,
                        reservationDto.ReservationTime
                    );

                    if (!isAvailable)
                        return null;

                    reservation.TableNumber = reservationDto.TableNumber;
                }
                // Otherwise, find an available table
                else
                {
                    var availableTables = await GetAvailableTablesAsync(
                        reservationDto.ReservationDate,
                        reservationDto.ReservationTime,
                        reservationDto.PartySize
                    );

                    if (!availableTables.Any())
                        return null;

                    reservation.TableNumber = availableTables.First().Number;
                }
            }

            // Update reservation details
            reservation.ReservationDate = reservationDto.ReservationDate;
            reservation.ReservationTime = reservationDto.ReservationTime;
            reservation.PartySize = reservationDto.PartySize;
            reservation.SpecialRequests = reservationDto.SpecialRequests;
            
            if (!string.IsNullOrEmpty(reservationDto.ContactPhone))
                reservation.ContactPhone = reservationDto.ContactPhone;
                
            if (!string.IsNullOrEmpty(reservationDto.ContactEmail))
                reservation.ContactEmail = reservationDto.ContactEmail;
                
            reservation.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _reservationRepository.UpdateAsync(reservation);

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<bool> UpdateReservationStatusAsync(string reservationId, ReservationStatus status)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                return false;

            reservation.Status = status;
            reservation.UpdatedAt = DateTime.UtcNow;

            var success = await _reservationRepository.UpdateAsync(reservation);

            if (success)
            {
                // Notify customer and staff about status change
                await _hubContext.Clients.User(reservation.CustomerId).SendAsync("ReservationStatusChanged", reservationId, status);
                await _hubContext.Clients.Groups(new[] { "Staff", "Admin" }).SendAsync("ReservationStatusChanged", reservationId, status);
            }

            return success;
        }

        public async Task<bool> CancelReservationAsync(string reservationId)
        {
            return await UpdateReservationStatusAsync(reservationId, ReservationStatus.Cancelled);
        }

        public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int partySize)
        {
            var tables = await _reservationRepository.GetAvailableTablesAsync(date, time, partySize);
            return _mapper.Map<List<AvailableTableDto>>(tables);
        }

        public async Task<bool> IsTableAvailableAsync(string tableNumber, DateTime date, TimeSpan time)
        {
            return await _reservationRepository.IsTableAvailableAsync(tableNumber, date, time);
        }
    }
}