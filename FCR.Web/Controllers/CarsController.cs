using FCR.Web.Models;
using FCR.Web.Services;
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
        private readonly ICarViewService _carViewService;

        public CarsController(IClient apiClient, ILogger<CarsController> logger, IWebHostEnvironment env, ICarViewService carViewService)
        {
            _apiClient = apiClient;
            _logger = logger;
            _env = env;
            _carViewService = carViewService;
        }

        // GET: Cars/Index
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? category,
            string? transmission,
            string? fuelType,
            bool? isAvailable,
            double? minPrice,
            double? maxPrice,
            int? minSeats)
        {
            var cars = await _carViewService.GetFilteredCarsAsync(
                searchTerm, category, transmission, fuelType, 
                isAvailable, minPrice, maxPrice, minSeats);

            // Pass filter values to view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
            ViewBag.Transmission = transmission;
            ViewBag.FuelType = fuelType;
            ViewBag.IsAvailable = isAvailable;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinSeats = minSeats;

            if (cars.Count == 0 && (searchTerm != null || category != null || transmission != null || fuelType != null))
            {
                TempData["InfoMessage"] = "No cars found matching your criteria.";
            }

            return View(cars);
        }
        // GET: Cars/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var car = await _carViewService.GetCarDetailsAsync(id);

            if (car == null)
            {
                TempData["ErrorMessage"] = "Car not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }
    }
}
