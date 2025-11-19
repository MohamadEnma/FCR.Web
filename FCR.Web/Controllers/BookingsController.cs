using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                var bookings = await _apiClient.MyBookingsAsync();
                return View(bookings);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading user bookings");
                ViewBag.ErrorMessage = "Unable to load your bookings.";
                return View(new List<Booking>());
            }
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create(int carId)
        {
            try
            {
                var car = await _apiClient.CarsGETAsync(carId);
                if (car == null)
                {
                    return NotFound();
                }

                ViewBag.Car = car;
                return View(new BookingCreateDto { CarId = carId });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car for booking");
                return NotFound();
            }
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                var car = await _apiClient.CarsGETAsync(model.CarId);
                ViewBag.Car = car;
                return View(model);
            }

            try
            {
                await _apiClient.BookingPOSTAsync(model);
                TempData["SuccessMessage"] = "Booking created successfully!";
                return RedirectToAction(nameof(MyBookings));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating booking");
                ModelState.AddModelError("", ex.Message ?? "Unable to create booking. Please try again.");

                var car = await _apiClient.CarsGETAsync(model.CarId);
                ViewBag.Car = car;
                return View(model);
            }
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var booking = await _apiClient.BookingGETAsync(id);
                return View(booking);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
            catch (ApiException ex) when (ex.StatusCode == 403)
            {
                return Forbid();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading booking details");
                return View("Error");
            }
        }

        // POST: Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _apiClient.CancelAsync(id);
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
                return RedirectToAction(nameof(MyBookings));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                TempData["ErrorMessage"] = "Unable to cancel booking. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}