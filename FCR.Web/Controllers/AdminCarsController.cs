using FCR.Web.Models;
using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCarsController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<AdminCarsController> _logger;
        private readonly IWebHostEnvironment _env;

        public AdminCarsController(IClient apiClient, ILogger<AdminCarsController> logger, IWebHostEnvironment env)
        {
            _apiClient = apiClient;
            _logger = logger;
            _env = env;
        }

        // GET: AdminCars/Index
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

        //  AdminCars/Create
        [HttpGet]
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

        // POST: AdminCars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm]CarCreateDto model, List<IFormFile>? uploadedImages, List<string>? imageUrls)
        {
            //  DEBUG: Log what's being received
            _logger.LogInformation("Received Brand: {Brand}", model.Brand);
            _logger.LogInformation("Received Model: {Model}", model.ModelName);
            _logger.LogInformation("Received Year: {Year}", model.Year);
            _logger.LogInformation("ModelState Valid: {IsValid}", ModelState.IsValid);

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {Error}", error.ErrorMessage);
            }



            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Prepare image URLs collection
                var allImageUrls = new List<string>();

                //  Declare fileParameters
                var fileParameters = new List<FileParameter>();

                // Add URL-based images if provided
                if (imageUrls != null && imageUrls.Any(url => !string.IsNullOrWhiteSpace(url)))
                {
                    allImageUrls.AddRange(imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)));
                }

                // Handle uploaded files
                if (uploadedImages != null && uploadedImages.Any())
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "cars");
                    Directory.CreateDirectory(uploadsFolder);

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var maxFileSize = 5 * 1024 * 1024; // 5MB

                    foreach (var file in uploadedImages)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] = $"Invalid file type: {file.FileName}. Only JPG, PNG, GIF, and WEBP are allowed.";
                            return View(model);
                        }

                        if (file.Length > maxFileSize)
                        {
                            TempData["ErrorMessage"] = $"File too large: {file.FileName}. Maximum size is 5MB.";
                            return View(model);
                        }

                        //  Create FileParameter for API
                        var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        fileParameters.Add(new FileParameter(memoryStream, file.FileName, file.ContentType));
                    }
                }

                // Call API with individual parameters
                var response = await _apiClient.CarsPOSTAsync(
                    brand: model.Brand,
                    modelName: model.ModelName,
                    category: model.Category,
                    year: model.Year,
                    dailyRate: (double)model.DailyRate,
                    weeklyRate: model.WeeklyRate.HasValue ? (double?)model.WeeklyRate.Value : null,
                    monthlyRate: model.MonthlyRate.HasValue ? (double?)model.MonthlyRate.Value : null,
                    transmission: model.Transmission,
                    fuelType: model.FuelType,
                    seats: model.Seats,
                    color: model.Color,
                    mileage: model.Mileage,
                    licensePlate: model.LicensePlate,
                    description: model.Description,
                    imageUrls: allImageUrls,
                    imageFiles: fileParameters
                );

                // ? Dispose streams
                foreach (var param in fileParameters)
                {
                    param.Data?.Dispose();
                }

                if (response?.Success == true && response.Data != null)
                {
                    TempData["SuccessMessage"] = $"Car '{model.Brand} {model.ModelName}' created successfully!";
                    return RedirectToAction(nameof(Index));
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

        // GET: AdminCars/Edit/5
        [HttpGet]
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
                ViewBag.CarId = car.CarId;
                ViewBag.ExistingImages = car.Images?.ToList() ?? new List<ImageResponseDto>();

                var updateDto = new CarUpdateDto
                {
                    CarId = car.CarId, 
                    Brand = car.Brand,
                    ModelName = car.ModelName,
                    Category = car.Category,
                    Year = car.Year,
                    DailyRate = (decimal)car.DailyRate,  
                    WeeklyRate = car.WeeklyRate.HasValue ? (decimal?)car.WeeklyRate.Value : null,
                    MonthlyRate = car.MonthlyRate.HasValue ? (decimal?)car.MonthlyRate.Value : null,
                    IsAvailable = car.IsAvailable,
                    Transmission = car.Transmission,
                    FuelType = car.FuelType,
                    Seats = car.Seats,
                    Color = car.Color,
                    Mileage = (int)car.Mileage,
                    LicensePlate = car.LicensePlate,
                    Description = car.Description
                };

                return View(updateDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car for edit");
                return NotFound();
            }
        }

        // POST: AdminCars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarUpdateDto model, List<IFormFile>? uploadedImages, List<string>? imageUrls)
        {
            if (id != model.CarId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var carResponse = await _apiClient.CarsGET2Async(id);
                ViewBag.CarId = id;
                ViewBag.ExistingImages = carResponse?.Data?.Images?.ToList() ?? new List<ImageResponseDto>();
                return View(model);
            }

            try
            {
                //Call CarsPUTAsync with all individual parameters
                var response = await _apiClient.CarsPUTAsync(
                    id: id,
                    brand: model.Brand,
                    modelName: model.ModelName,
                    category: model.Category,
                    year: model.Year,
                    dailyRate: (double)model.DailyRate,
                    weeklyRate: model.WeeklyRate.HasValue ? (double?)model.WeeklyRate.Value : null,
                    monthlyRate: model.MonthlyRate.HasValue ? (double?)model.MonthlyRate.Value : null,
                    isAvailable: model.IsAvailable,
                    transmission: model.Transmission,
                    fuelType: model.FuelType,
                    seats: model.Seats,
                    color: model.Color,
                    mileage: model.Mileage,
                    licensePlate: model.LicensePlate,
                    description: model.Description,
                    imageUrls: model.ImageUrls ?? new List<string>(),
                    imageFiles: new List<FileParameter>()  // Empty for now, images handled separately below
                );

                if (response?.Success != true)
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to update car.";
                    var carResponse = await _apiClient.CarsGET2Async(id);
                    ViewBag.CarId = id;
                    ViewBag.ExistingImages = carResponse?.Data?.Images?.ToList() ?? new List<ImageResponseDto>();
                    return View(model);
                }

                // Handle new image uploads
                if (uploadedImages != null && uploadedImages.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var maxFileSize = 5 * 1024 * 1024; // 5MB
                    var fileParameters = new List<FileParameter>();

                    foreach (var file in uploadedImages)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] = $"Invalid file type: {file.FileName}. Only JPG, PNG, GIF, and WEBP are allowed.";
                            return RedirectToAction(nameof(Edit), new { id });
                        }

                        if (file.Length > maxFileSize)
                        {
                            TempData["ErrorMessage"] = $"File too large: {file.FileName}. Maximum size is 5MB.";
                            return RedirectToAction(nameof(Edit), new { id });
                        }

                        var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        fileParameters.Add(new FileParameter(memoryStream, file.FileName, file.ContentType));
                    }

                    if (fileParameters.Any())
                    {
                        var imageResponse = await _apiClient.ImagesPOSTAsync(id, fileParameters);

                        foreach (var param in fileParameters)
                        {
                            param.Data?.Dispose();
                        }

                        if (imageResponse?.Success != true)
                        {
                            TempData["WarningMessage"] = "Car updated but some images failed to upload.";
                        }
                    }
                }

                TempData["SuccessMessage"] = $"Car '{model.Brand} {model.ModelName}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error updating car");
                TempData["ErrorMessage"] = "Error updating car. Please try again.";
                return RedirectToAction(nameof(Edit), new { id });
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

        // GET: AdminCars/Delete/5
        [HttpGet]
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

        // POST: AdminCars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _apiClient.CarsDELETEAsync(id);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Car deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to delete car.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting car");
                TempData["ErrorMessage"] = "Error deleting car. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // POST: AdminCars/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int carId, int imageId)
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

                return RedirectToAction(nameof(Edit), new { id = carId });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting image");
                TempData["ErrorMessage"] = "Error deleting image. Please try again.";
                return RedirectToAction(nameof(Edit), new { id = carId });
            }
        }

        // POST: AdminCars/SetPrimaryImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryImage(int carId, int imageId)
        {
            try
            {
                var response = await _apiClient.PrimaryAsync(carId, imageId);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Primary image set successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to set primary image.";
                }

                return RedirectToAction(nameof(Edit), new { id = carId });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error setting primary image");
                TempData["ErrorMessage"] = "Error setting primary image. Please try again.";
                return RedirectToAction(nameof(Edit), new { id = carId });
            }
        }
    }
}