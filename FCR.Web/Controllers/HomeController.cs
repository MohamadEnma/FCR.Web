using FCR.Web.Services.Base;
using FCR.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cars = await _apiClient.CarsAllAsync();
                var viewModel = new HomeViewModel
                {
                    AllCars = cars.ToList()
                };
                return View(viewModel);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for home page");
                ViewBag.ErrorMessage = "Unable to load cars. Please try again later.";
                return View(new HomeViewModel { AllCars = new List<Car>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading home page");
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                return View(new HomeViewModel { AllCars = new List<Car>() });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}