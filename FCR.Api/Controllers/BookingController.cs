using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Booking;
using FCR.Bll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings for the authenticated user
        /// </summary>
        [HttpGet("my-bookings")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _bookingService.GetUserBookingsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get active bookings for the authenticated user
        /// </summary>
        [HttpGet("my-bookings/active")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyActiveBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _bookingService.GetActiveBookingsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get upcoming bookings for the authenticated user
        /// </summary>
        [HttpGet("my-bookings/upcoming")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyUpcomingBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _bookingService.GetUpcomingBookingsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all active bookings (Admin only)
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllActiveBookings()
        {
            var result = await _bookingService.GetBookingsByStatusAsync("Confirmed");

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _bookingService.CreateBookingAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Booking created successfully for user {UserId}, Car {CarId}", userId, dto.CarId);

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = result.Data?.BookingId },
                result);
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _bookingService.GetBookingByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            // Only admin or booking owner can view
            if (!User.IsInRole("Admin") && result.Data?.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to access booking {BookingId}", userId, id);
                return Forbid();
            }

            return Ok(result);
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto? dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            // Check ownership
            var bookingResult = await _bookingService.GetBookingByIdAsync(id);
            if (!bookingResult.Success)
                return NotFound(bookingResult);

            // Only admin or booking owner can cancel
            if (!User.IsInRole("Admin") && bookingResult.Data?.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to cancel booking {BookingId}", userId, id);
                return Forbid();
            }

            var result = await _bookingService.CancelBookingAsync(id, userId, dto?.Reason);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Booking {BookingId} cancelled by user {UserId}", id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Check car availability for specific dates
        /// </summary>
        [HttpPost("check-availability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _bookingService.CheckCarAvailabilityAsync(
                dto.CarId,
                dto.PickupDate,
                dto.ReturnDate);

            return Ok(result);
        }

        /// <summary>
        /// Calculate booking price
        /// </summary>
        [HttpPost("calculate-price")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<decimal>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculatePrice([FromBody] BookingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<decimal>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _bookingService.CalculateBookingPriceAsync(
                dto.CarId,
                dto.PickupDate,
                dto.ReturnDate);

            return Ok(result);
        }

        /// <summary>
        /// Confirm booking (Admin only)
        /// </summary>
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var result = await _bookingService.ConfirmBookingAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Booking {BookingId} confirmed by admin", id);
            return Ok(result);
        }

        /// <summary>
        /// Complete booking (Admin only)
        /// </summary>
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteBooking(int id)
        {
            var result = await _bookingService.CompleteBookingAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Booking {BookingId} completed by admin", id);
            return Ok(result);
        }

        /// <summary>
        /// Delete booking (Admin only - soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogWarning("Booking {BookingId} deleted by admin", id);
            return Ok(result);
        }
    }
}