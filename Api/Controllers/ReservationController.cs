using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Reservation;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(string id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            
            if (reservation == null)
                return NotFound(new ErrorResponse("Reservation not found"));
                
            return Ok(ApiResponse<ReservationDto>.SuccessResponse(reservation));
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetReservationsByCustomerId(string customerId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Customers can only access their own reservations
            if (userRole == "Customer" && userId != customerId)
                return Forbid();
                
            var reservations = await _reservationService.GetReservationsByCustomerIdAsync(customerId);
            return Ok(ApiResponse<List<ReservationDto>>.SuccessResponse(reservations));
        }

        [HttpGet("date/{date}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetReservationsByDate(DateTime date)
        {
            var reservations = await _reservationService.GetReservationsByDateAsync(date);
            return Ok(ApiResponse<List<ReservationDto>>.SuccessResponse(reservations));
        }

        [HttpGet("upcoming")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetUpcomingReservations()
        {
            var reservations = await _reservationService.GetUpcomingReservationsAsync();
            return Ok(ApiResponse<List<ReservationDto>>.SuccessResponse(reservations));
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto reservationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));
                
            var reservationId = await _reservationService.CreateReservationAsync(reservationDto, userId);
            
            if (string.IsNullOrEmpty(reservationId))
                return BadRequest(new ErrorResponse("Failed to create reservation"));
                
            var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
            return CreatedAtAction(nameof(GetReservationById), new { id = reservationId }, ApiResponse<ReservationDto>.SuccessResponse(reservation, "Reservation created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(string id, [FromBody] UpdateReservationDto reservationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Check if customer is updating their own reservation
            if (userRole == "Customer")
            {
                var existingReservation = await _reservationService.GetReservationByIdAsync(id);
                if (existingReservation?.CustomerId != userId)
                    return Forbid();
            }
            
            var updatedReservation = await _reservationService.UpdateReservationAsync(id, reservationDto);
            
            if (updatedReservation == null)
                return NotFound(new ErrorResponse("Reservation not found or update failed"));
                
            return Ok(ApiResponse<ReservationDto>.SuccessResponse(updatedReservation, "Reservation updated successfully"));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateReservationStatus(string id, [FromBody] UpdateReservationStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var success = await _reservationService.UpdateReservationStatusAsync(id, statusDto.Status);
            
            if (!success)
                return NotFound(new ErrorResponse("Reservation not found or status update failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Reservation status updated successfully"));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Check if customer is cancelling their own reservation
            if (userRole == "Customer")
            {
                var existingReservation = await _reservationService.GetReservationByIdAsync(id);
                if (existingReservation?.CustomerId != userId)
                    return Forbid();
            }
            
            var success = await _reservationService.CancelReservationAsync(id);
            
            if (!success)
                return NotFound(new ErrorResponse("Reservation not found or cancellation failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Reservation cancelled successfully"));
        }

        [HttpPost("available-tables")]
        public async Task<IActionResult> GetAvailableTables([FromBody] CheckAvailabilityDto checkDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var tables = await _reservationService.GetAvailableTablesAsync(checkDto.Date, checkDto.Time, checkDto.PartySize);
            return Ok(ApiResponse<List<AvailableTableDto>>.SuccessResponse(tables));
        }

        [HttpPost("check-table")]
        public async Task<IActionResult> CheckTableAvailability([FromBody] TableAvailabilityDto checkDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var isAvailable = await _reservationService.IsTableAvailableAsync(checkDto.TableNumber, checkDto.Date, checkDto.Time);
            return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
        }
    }

    public class TableAvailabilityDto
    {
        public string TableNumber { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}