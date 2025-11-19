using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Web.Controllers
{
    public class CarsController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<CarsController> _logger;

        public CarsController(IClient apiClient, ILogger<CarsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: Cars
        public async Task<IActionResult> Index(string category, string transmission, string fuelType, int? minSeats, double? maxDailyRate)
        {
            try
            {
                IEnumerable<Car> cars;

                if (string.IsNullOrEmpty(category) && string.IsNullOrEmpty(transmission) &&
                    string.IsNullOrEmpty(fuelType) && !minSeats.HasValue && !maxDailyRate.HasValue)
                {
                    cars = await _apiClient.CarsAllAsync();
                }
                else
                {
                    cars = await _apiClient.FilterAsync(category, transmission, fuelType, minSeats, maxDailyRate);
                }

                ViewBag.Category = category;
                ViewBag.Transmission = transmission;
                ViewBag.FuelType = fuelType;
                ViewBag.MinSeats = minSeats;
                ViewBag.MaxDailyRate = maxDailyRate;

                return View(cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars");
                ViewBag.ErrorMessage = "Unable to load cars. Please try again later.";
                return View(new List<Car>());
            }
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var car = await _apiClient.CarsGETAsync(id);
                if (car == null)
                {
                    return NotFound();
                }
                return View(car);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car details for ID {CarId}", id);
                ViewBag.ErrorMessage = "Unable to load car details.";
                return View("Error");
            }
        }

        // GET: Cars/Available
        public async Task<IActionResult> Available()
        {
            try
            {
                var cars = await _apiClient.AvailableAsync();
                return View("Index", cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading available cars");
                ViewBag.ErrorMessage = "Unable to load available cars.";
                return View("Index", new List<Car>());
            }
        }
    }
}