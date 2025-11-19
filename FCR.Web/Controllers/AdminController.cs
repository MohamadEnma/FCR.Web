using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IClient apiClient, ILogger<AdminController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var activeBookings = await _apiClient.ActiveAsync();
                return View(activeBookings);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                ViewBag.ErrorMessage = "Unable to load dashboard data.";
                return View(new List<Booking>());
            }
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _apiClient.UsersAllAsync();
                return View(users);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading users");
                ViewBag.ErrorMessage = "Unable to load users.";
                return View(new List<UserDto>());
            }
        }

        // GET: Admin/ManageCars
        public async Task<IActionResult> ManageCars()
        {
            try
            {
                var cars = await _apiClient.CarsAllAsync();
                return View(cars);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for management");
                ViewBag.ErrorMessage = "Unable to load cars.";
                return View(new List<Car>());
            }
        }

        // POST: Admin/DeleteCar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                await _apiClient.CarsDELETEAsync(id);
                TempData["SuccessMessage"] = "Car deleted successfully.";
                return RedirectToAction(nameof(ManageCars));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting car");
                TempData["ErrorMessage"] = "Unable to delete car.";
                return RedirectToAction(nameof(ManageCars));
            }
        }
    }
}