using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Index(string? category, string? transmission, string? fuelType, int? minSeats, double? maxDailyRate)
        {
            try
            {
                IEnumerable<CarResponseDto> cars;

                if (string.IsNullOrEmpty(category) && string.IsNullOrEmpty(transmission) &&
                    string.IsNullOrEmpty(fuelType) && !minSeats.HasValue && !maxDailyRate.HasValue)
                {
                    // Get all cars
                    var response = await _apiClient.CarsGETAsync();
                    cars = response?.Data ?? new List<CarResponseDto>();
                }
                else
                {
                    // Filter cars
                    var response = await _apiClient.FilterAsync(category, transmission, fuelType, minSeats, maxDailyRate);
                    cars = response?.Data ?? new List<CarResponseDto>();
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
                return View(new List<CarResponseDto>());
            }
        }

        // GET: Cars/Details/5
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
                ViewBag.ErrorMessage = "Unable to load car details.";
                return View("Error");
            }
        }

        // GET: Cars/Available
        public async Task<IActionResult> Available()
        {
            try
            {
                var response = await _apiClient.AvailableAsync();
                var cars = response?.Data ?? new List<CarResponseDto>();
                return View("Index", cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading available cars");
                ViewBag.ErrorMessage = "Unable to load available cars.";
                return View("Index", new List<CarResponseDto>());
            }
        }

        // GET: Cars/Search?keyword=bmw
        public async Task<IActionResult> Search(string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return RedirectToAction(nameof(Index));
                }

                var response = await _apiClient.SearchAsync(keyword);
                var cars = response?.Data ?? new List<CarResponseDto>();

                ViewBag.SearchKeyword = keyword;
                return View("Index", cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error searching cars with keyword {Keyword}", keyword);
                ViewBag.ErrorMessage = "Unable to search cars.";
                return View("Index", new List<CarResponseDto>());
            }
        }

        // GET: Cars/Category/SUV
        public async Task<IActionResult> Category(string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    return RedirectToAction(nameof(Index));
                }

                var response = await _apiClient.CategoryAsync(category);
                var cars = response?.Data ?? new List<CarResponseDto>();

                ViewBag.Category = category;
                return View("Index", cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for category {Category}", category);
                ViewBag.ErrorMessage = "Unable to load cars for this category.";
                return View("Index", new List<CarResponseDto>());
            }
        }

        // GET: Cars/Brand/BMW
        public async Task<IActionResult> Brand(string brand)
        {
            try
            {
                if (string.IsNullOrEmpty(brand))
                {
                    return RedirectToAction(nameof(Index));
                }

                var response = await _apiClient.BrandAsync(brand);
                var cars = response?.Data ?? new List<CarResponseDto>();

                ViewBag.Brand = brand;
                return View("Index", cars.ToList());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for brand {Brand}", brand);
                ViewBag.ErrorMessage = "Unable to load cars for this brand.";
                return View("Index", new List<CarResponseDto>());
            }
        }

        // GET: Cars/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new CarCreateDto
            {
             
                Transmission = "Automatic",
                FuelType = "Petrol",
                Seats = 5,
                Year = DateTime.Now.Year
            };

            return View(model);
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CarCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiClient.CarsPOSTAsync(model);

                if (response?.Success == true && response.Data != null)
                {
                    TempData["SuccessMessage"] = $"Car '{model.Brand} {model.Model}' created successfully!";

                    // Redirect to image upload page
                    return RedirectToAction("UploadImages", new { id = response.Data.CarId });
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to create car.";
                    return View(model);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating car");
                TempData["ErrorMessage"] = "Error creating car. Please try again.";
                return View(model);
            }
        }

        // GET: Cars/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
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
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car for deletion");
                return NotFound();
            }
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int carId)
        {
            try
            {
                var response = await _apiClient.CarsDELETEAsync(carId);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Car deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to delete car.";
                    return RedirectToAction(nameof(Delete), new { id = carId });
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting car");
                TempData["ErrorMessage"] = "Error deleting car. Please try again.";
                return RedirectToAction(nameof(Delete), new { id = carId });
            }
        }

        // GET: Cars/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _apiClient.CarsGET2Async(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                var car = response.Data;

                // Pass data via ViewBag for display
                ViewBag.CarId = car.CarId;
                ViewBag.Brand = car.Brand;
                ViewBag.Model = car.Model;

                // Create update DTO
                var updateDto = new CarUpdateDto
                {
                    Brand = car.Brand,
                    Model = car.Model,
                    Category = car.Category,
                    Year = car.Year,
                    DailyRate = car.DailyRate,
                    IsAvailable = car.IsAvailable,
                    Transmission = car.Transmission,
                    FuelType = car.FuelType,
                    Seats = car.Seats
                };

                return View(updateDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car for edit");
                return NotFound();
            }
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int carId, CarUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                // Reload car info for display
                var carResponse = await _apiClient.CarsGET2Async(carId);
                ViewBag.CarId = carId;
                ViewBag.Brand = carResponse?.Data?.Brand;
                ViewBag.Model = carResponse?.Data?.Model;
                return View(model);
            }

            try
            {
                var response = await _apiClient.CarsPUTAsync(carId, model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = $"Car '{model.Brand} {model.Model}' updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = carId });
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to update car.";

                    // Reload data
                    var carResponse = await _apiClient.CarsGET2Async(carId);
                    ViewBag.CarId = carId;
                    ViewBag.Brand = carResponse?.Data?.Brand;
                    ViewBag.Model = carResponse?.Data?.Model;

                    return View(model);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error updating car");
                TempData["ErrorMessage"] = "Error updating car. Please try again.";

                // Reload data
                var carResponse = await _apiClient.CarsGET2Async(carId);
                ViewBag.CarId = carId;
                ViewBag.Brand = carResponse?.Data?.Brand;
                ViewBag.Model = carResponse?.Data?.Model;

                return View(model);
            }
        }

        // GET: Cars/UploadImages/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImages(int id)
        {
            try
            {
                var response = await _apiClient.CarsGET2Async(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                ViewBag.CarId = id;
                ViewBag.Brand = response.Data.Brand;
                ViewBag.Model = response.Data.Model;
                ViewBag.Images = response.Data.Images?.ToList() ?? new List<ImageResponseDto>();

                return View();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car images page");
                return NotFound();
            }
        }

        // POST: Cars/UploadImages - FILE UPLOAD
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImages(int carId, List<IFormFile> images)
        {
            if (images == null || !images.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one image file.";
                return RedirectToAction(nameof(UploadImages), new { id = carId });
            }

            // Validate file types and sizes
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxFileSize = 5 * 1024 * 1024; // 5MB

            foreach (var image in images)
            {
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = $"Invalid file type: {image.FileName}. Only JPG, PNG, and GIF are allowed.";
                    return RedirectToAction(nameof(UploadImages), new { id = carId });
                }

                if (image.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] = $"File too large: {image.FileName}. Maximum size is 5MB.";
                    return RedirectToAction(nameof(UploadImages), new { id = carId });
                }
            }

            try
            {
                // Convert IFormFiles to FileParameter for NSwag client
                var fileParameters = new List<FileParameter>();

                foreach (var image in images)
                {
                    var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    fileParameters.Add(new FileParameter(memoryStream, image.FileName, image.ContentType));
                }

                // Call API with file uploads
                var response = await _apiClient.ImagesPOSTAsync(carId, fileParameters);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = $"{images.Count} image(s) uploaded successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to upload images.";
                }

                // Clean up streams
                foreach (var param in fileParameters)
                {
                    param.Data.Dispose();
                }

                return RedirectToAction(nameof(UploadImages), new { id = carId });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error uploading images");
                TempData["ErrorMessage"] = $"Error uploading images: {ex.Message}";
                return RedirectToAction(nameof(UploadImages), new { id = carId });
            }
        }

        // POST: Cars/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int imageId, int carId)
        {
            try
            {
                var response = await _apiClient.ImagesDELETEAsync(carId, imageId);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Image deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to delete image.";
                }

                return RedirectToAction(nameof(UploadImages), new { id = carId });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting image");
                TempData["ErrorMessage"] = "Error deleting image. Please try again.";
                return RedirectToAction(nameof(UploadImages), new { id = carId });
            }
        }
    }
}