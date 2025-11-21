using FCR.Web.Models;
using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FCR.Web.Controllers
{
    public class CarsController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<CarsController> _logger;
        private readonly IWebHostEnvironment _env;

        public CarsController(IClient apiClient, ILogger<CarsController> logger, IWebHostEnvironment env)
        {
            _apiClient = apiClient;
            _logger = logger;
            _env = env;
        }

        // GET:Cars/Index
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? category,
            string? transmission,
            string? fuelType,
            bool? isAvailable,
            double? minPrice,
            double? maxPrice,
            int? minSeats,
            int pageNumber = 1,
            int pageSize = 12)
        {
            try
            {
                IEnumerable<CarResponseDto> cars;

                // Apply filters if any
                if (!string.IsNullOrEmpty(category) || !string.IsNullOrEmpty(transmission) ||
                    !string.IsNullOrEmpty(fuelType) || minSeats.HasValue || maxPrice.HasValue)
                {
                    var response = await _apiClient.FilterAsync(category, transmission, fuelType, minSeats, maxPrice);
                    cars = response?.Data ?? new List<CarResponseDto>();
                }
                else
                {
                    var response = await _apiClient.CarsGETAsync();
                    cars = response?.Data ?? new List<CarResponseDto>();
                }

                // Apply search term filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cars = cars.Where(c =>
                        c.Brand?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        c.ModelName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
                }

                // Apply availability filter
                if (isAvailable.HasValue)
                {
                    cars = cars.Where(c => c.IsAvailable == isAvailable.Value);
                }

                // Apply min price filter
                if (minPrice.HasValue)
                {
                    cars = cars.Where(c => c.DailyRate >= minPrice.Value);
                }

                // Pass filter values to view
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Category = category;
                ViewBag.Transmission = transmission;
                ViewBag.FuelType = fuelType;
                ViewBag.IsAvailable = isAvailable;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.MinSeats = minSeats;

                return View(cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars in admin panel");
                TempData["ErrorMessage"] = "Unable to load cars. Please try again later.";
                return View(new List<CarResponseDto>());
            }
        }
        // GET: AdminCars/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiClient.CarsGET2Async(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car details for ID {CarId}", id);
                TempData["ErrorMessage"] = "Unable to load car details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
