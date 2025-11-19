using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCR.Web.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IClient apiClient, ILogger<BookingsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var response = await _apiClient.MyBookingsAsync();
                var bookings = response?.Data ?? new List<BookingResponseDto>();
                return View(bookings);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading my bookings");
                TempData["ErrorMessage"] = "Unable to load bookings. Please try again.";
                return View(new List<BookingResponseDto>());
            }
        }

        // GET: Bookings/Index (Admin - All Bookings)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.BookingsAsync();
                var bookings = response?.Data ?? new List<BookingResponseDto>();
                return View(bookings);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading all bookings");
                TempData["ErrorMessage"] = "Unable to load bookings. Please try again.";
                return View(new List<BookingResponseDto>());
            }
        }

        // GET: Bookings/Create?carId=1
        [HttpGet]
        public async Task<IActionResult> Create(int carId)
        {
            try
            {
                var carResponse = await _apiClient.CarsGET2Async(carId);

                if (carResponse?.Data == null)
                {
                    TempData["ErrorMessage"] = "Car not found.";
                    return RedirectToAction("Index", "Cars");
                }

                if (!carResponse.Data.IsAvailable)
                {
                    TempData["ErrorMessage"] = "This car is not available for booking.";
                    return RedirectToAction("Details", "Cars", new { id = carId });
                }

                // Pass car data via ViewBag
                ViewBag.Car = carResponse.Data;
                ViewBag.CarImages = carResponse.Data.Images?.ToList() ?? new List<ImageResponseDto>();
                ViewBag.CarBrand = carResponse.Data.Brand;
                ViewBag.CarModel = carResponse.Data.Model;

                var model = new BookingCreateDto
                {
                    CarId = carId,
                    PickupDate = DateTime.Today.AddDays(1),
                    ReturnDate = DateTime.Today.AddDays(3)
                };

                return View(model);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car for booking");
                TempData["ErrorMessage"] = "Unable to load car details.";
                return RedirectToAction("Index", "Cars");
            }
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                // Reload car data
                var carResponse = await _apiClient.CarsGET2Async(model.CarId);
                ViewBag.Car = carResponse?.Data;
                ViewBag.CarImages = carResponse?.Data?.Images?.ToList() ?? new List<ImageResponseDto>();
                ViewBag.CarBrand = carResponse?.Data?.Brand;
                ViewBag.CarModel = carResponse?.Data?.Model;

                return View(model);
            }

            try
            {
                // FIXED: Use BookingPOSTAsync (no 's')
                var response = await _apiClient.BookingPOSTAsync(model);

                if (response?.Success == true && response.Data != null)
                {
                    return RedirectToAction(nameof(Confirmation), new { id = response.Data.BookingId });
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to create booking.";

                    // Reload car data
                    var carResponse = await _apiClient.CarsGET2Async(model.CarId);
                    ViewBag.Car = carResponse?.Data;
                    ViewBag.CarImages = carResponse?.Data?.Images?.ToList() ?? new List<ImageResponseDto>();
                    ViewBag.CarBrand = carResponse?.Data?.Brand;
                    ViewBag.CarModel = carResponse?.Data?.Model;

                    return View(model);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating booking");
                TempData["ErrorMessage"] = "Error creating booking. Please try again.";

                // Reload car data
                var carResponse = await _apiClient.CarsGET2Async(model.CarId);
                ViewBag.Car = carResponse?.Data;
                ViewBag.CarImages = carResponse?.Data?.Images?.ToList() ?? new List<ImageResponseDto>();
                ViewBag.CarBrand = carResponse?.Data?.Brand;
                ViewBag.CarModel = carResponse?.Data?.Model;

                return View(model);
            }
        }

        // GET: Bookings/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            try
            {
                var response = await _apiClient.BookingGETAsync(id);

                if (response?.Data == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction(nameof(MyBookings));
                }

                // Verify the booking belongs to the current user (unless admin)
                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (response.Data.UserId != currentUserId)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to view this booking.";
                        return RedirectToAction(nameof(MyBookings));
                    }
                }

                return View(response.Data);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading booking confirmation");
                TempData["ErrorMessage"] = "Unable to load booking details.";
                return RedirectToAction(nameof(MyBookings));
            }
        }

        // GET: Bookings/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiClient.BookingGETAsync(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                // Verify the booking belongs to the current user (unless admin)
                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (response.Data.UserId != currentUserId)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to view this booking.";
                        return RedirectToAction(nameof(MyBookings));
                    }
                }

                return View(response.Data);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading booking details");
                return NotFound();
            }
        }

        // GET: Bookings/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiClient.BookingGETAsync(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                // Verify the booking belongs to the current user (unless admin)
                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (response.Data.UserId != currentUserId)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to cancel this booking.";
                        return RedirectToAction(nameof(MyBookings));
                    }
                }

                return View(response.Data);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading booking for deletion");
                return NotFound();
            }
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int bookingId)
        {
            try
            {
                // Use Cancel method with DTO
                var cancelDto = new CancelBookingDto
                {
                    Reason = "Cancelled by user"
                };
                var response = await _apiClient.CancelAsync(bookingId, cancelDto);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully.";
                    return RedirectToAction(nameof(MyBookings));
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to cancel booking.";
                    return RedirectToAction(nameof(Delete), new { id = bookingId });
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                TempData["ErrorMessage"] = "Error cancelling booking. Please try again.";
                return RedirectToAction(nameof(Delete), new { id = bookingId });
            }
        }

        // POST: Bookings/Cancel (Alternative method for forms)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var cancelDto = new CancelBookingDto
                {
                    Reason = "Cancelled by user"
                };
                var response = await _apiClient.CancelAsync(id, cancelDto);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to cancel booking.";
                }

                return RedirectToAction(nameof(MyBookings));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                TempData["ErrorMessage"] = "Error cancelling booking. Please try again.";
                return RedirectToAction(nameof(MyBookings));
            }
        }
    }
}