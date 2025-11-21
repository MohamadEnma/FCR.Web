using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Booking;
using FCR.Bll.DTOs.Car;
using FCR.Bll.DTOs.User;
using FCR.Bll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCR.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            ICarService carService,
            IBookingService bookingService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _carService = carService;
            _bookingService = bookingService;
            _logger = logger;
        }

        // ========== USER MANAGEMENT ==========

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Admin retrieved all users");
            return Ok(result);
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Update user details (Admin only)
        /// </summary>
        [HttpPut("users/{id}")]
        [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<UserDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _userService.UpdateUserAsync(id, dto);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Admin updated user: {UserId}", id);
            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Prevent admin from deleting themselves
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Cannot delete own account",
                    "Admins cannot delete their own accounts"));

            // Use fast admin delete
            var result = await _userService.AdminDeleteUserAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogWarning("Admin deleted user: {UserId}", id);
            return Ok(result);
        }

        // ========== BOOKING MANAGEMENT ==========

        /// <summary>
        /// Get all bookings (Admin only)
        /// </summary>
        [HttpGet("bookings")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllBookings()
        {
            var result = await _bookingService.GetAllBookingsAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get bookings by status (Admin only)
        /// </summary>
        [HttpGet("bookings/status/{status}")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BookingResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingsByStatus(string status)
        {
            var result = await _bookingService.GetBookingsByStatusAsync(status);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update booking status (Admin only)
        /// </summary>
        [HttpPut("bookings/{id}/status")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _bookingService.UpdateBookingStatusAsync(id, dto.Status);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Admin updated booking {BookingId} status to {Status}", id, dto.Status);
            return Ok(result);
        }

        /// <summary>
        /// Confirm booking (Admin only)
        /// </summary>
        [HttpPut("bookings/{id}/confirm")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var result = await _bookingService.ConfirmBookingAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Admin confirmed booking: {BookingId}", id);
            return Ok(result);
        }

        /// <summary>
        /// Complete booking (Admin only)
        /// </summary>
        [HttpPut("bookings/{id}/complete")]
        [ProducesResponseType(typeof(ServiceResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteBooking(int id)
        {
            var result = await _bookingService.CompleteBookingAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Admin completed booking: {BookingId}", id);
            return Ok(result);
        }

        // ========== CAR MANAGEMENT ==========

        /// <summary>
        /// Update car availability (Admin only)
        /// </summary>
        [HttpPut("cars/{id}/availability")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCarAvailability(int id, [FromBody] UpdateCarAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _carService.UpdateCarAvailabilityAsync(id, dto.IsAvailable);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Admin updated car {CarId} availability to {IsAvailable}", id, dto.IsAvailable);
            return Ok(result);
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Get dashboard statistics (Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AdminStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            var totalBookingsResult = await _bookingService.GetTotalBookingsCountAsync();
            var totalRevenueResult = await _bookingService.GetTotalRevenueAsync();
            var allUsersResult = await _userService.GetAllUsersAsync();
            var allCarsResult = await _carService.GetAllCarsAsync();

            var statistics = new AdminStatisticsDto
            {
                TotalUsers = allUsersResult.Data?.Count() ?? 0,
                TotalCars = allCarsResult.Data?.Count() ?? 0,
                TotalBookings = totalBookingsResult.Data,
                TotalRevenue = totalRevenueResult.Data
            };

            return Ok(ServiceResponse<AdminStatisticsDto>.SuccessResponse(statistics));
        }
    }
}