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
                var response = await _apiClient.CarsGETAsync();
                var viewModel = new HomeViewModel
                {
                    AllCars = response?.Data?.ToList() ?? new List<CarResponseDto>()
                };
                return View(viewModel);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for home page");
                ViewBag.ErrorMessage = "Unable to load cars. Please try again later.";
                return View(new HomeViewModel { AllCars = new List<CarResponseDto>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading home page");
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                return View(new HomeViewModel { AllCars = new List<CarResponseDto>() });
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
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