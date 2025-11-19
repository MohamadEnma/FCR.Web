using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Car;
using FCR.Bll.DTOs.Image;
using FCR.Bll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IImageService _imageService;
        private readonly ILogger<CarsController> _logger;

        public CarsController(
            ICarService carService,
            IImageService imageService,
            ILogger<CarsController> logger)
        {
            _carService = carService;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available cars
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCars()
        {
            var result = await _carService.GetAvailableCarsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all cars with pagination
        /// </summary>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(ServiceResponse<PagedResult<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCarsPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _carService.GetAllCarsPaginatedAsync(paginationParams);
            return Ok(result);
        }

        /// <summary>
        /// Get car by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCar(int id)
        {
            var result = await _carService.GetCarByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get cars by brand
        /// </summary>
        [HttpGet("brand/{brand}")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCarsByBrand(string brand)
        {
            var result = await _carService.GetCarsByBrandAsync(brand);
            return Ok(result);
        }

        /// <summary>
        /// Get cars by category
        /// </summary>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCarsByCategory(string category)
        {
            var result = await _carService.GetCarsByCategoryAsync(category);
            return Ok(result);
        }

        /// <summary>
        /// Get available cars
        /// </summary>
        [HttpGet("available")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableCars()
        {
            var result = await _carService.GetAvailableCarsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Search cars by keyword
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCars([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Invalid search",
                    "Search keyword is required"));

            var result = await _carService.SearchCarsAsync(keyword);
            return Ok(result);
        }

        /// <summary>
        /// Filter cars with multiple criteria
        /// </summary>
        [HttpGet("filter")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FilterCars(
            [FromQuery] string? category = null,
            [FromQuery] string? transmission = null,
            [FromQuery] string? fuelType = null,
            [FromQuery] int? minSeats = null,
            [FromQuery] decimal? maxDailyRate = null)
        {
            var result = await _carService.FilterCarsAsync(
                category,
                transmission,
                fuelType,
                minSeats,
                maxDailyRate);

            return Ok(result);
        }

        /// <summary>
        /// Filter cars with pagination
        /// </summary>
        [HttpGet("filter/paginated")]
        [ProducesResponseType(typeof(ServiceResponse<PagedResult<CarResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FilterCarsPaginated(
            [FromQuery] string? category = null,
            [FromQuery] string? transmission = null,
            [FromQuery] string? fuelType = null,
            [FromQuery] int? minSeats = null,
            [FromQuery] decimal? maxDailyRate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _carService.FilterCarsPaginatedAsync(
                category,
                transmission,
                fuelType,
                minSeats,
                maxDailyRate,
                paginationParams);

            return Ok(result);
        }

        /// <summary>
        /// Create a new car (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCar([FromBody] CarCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<CarResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _carService.CreateCarAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Car created: {Brand} {Model}", dto.Brand, dto.Model);

            return CreatedAtAction(
                nameof(GetCar),
                new { id = result.Data?.CarId },
                result);
        }

        /// <summary>
        /// Update car details (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServiceResponse<CarResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<CarResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _carService.UpdateCarAsync(id, dto);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Car updated: {CarId}", id);
            return Ok(result);
        }

        /// <summary>
        /// Update car availability (Admin only)
        /// </summary>
        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] UpdateCarAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _carService.UpdateCarAvailabilityAsync(id, dto.IsAvailable);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Car {CarId} availability updated to {IsAvailable}", id, dto.IsAvailable);
            return Ok(result);
        }

        /// <summary>
        /// Delete car - soft delete (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var result = await _carService.DeleteCarAsync(id);

            if (!result.Success)
                return NotFound(result);

            _logger.LogWarning("Car {CarId} deleted (soft delete)", id);
            return Ok(result);
        }

        // ========== IMAGE MANAGEMENT ==========

        /// <summary>
        /// Upload images for a car (Admin only)
        /// </summary>
        [HttpPost("{carId}/images")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ImageResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ImageResponseDto>>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadCarImages(int carId, [FromForm] List<IFormFile> images)
        {
            if (images == null || !images.Any())
                return BadRequest(ServiceResponse<IEnumerable<ImageResponseDto>>.ErrorResponse(
                    "No images provided",
                    "At least one image is required"));

            var result = await _imageService.UploadMultipleImagesAsync(carId, images);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Uploaded {Count} images for car {CarId}", images.Count, carId);
            return Ok(result);
        }

        /// <summary>
        /// Delete car image (Admin only)
        /// </summary>
        [HttpDelete("{carId}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCarImage(int carId, int imageId)
        {
            var result = await _imageService.DeleteImageAsync(imageId);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Deleted image {ImageId} from car {CarId}", imageId, carId);
            return Ok(result);
        }

        /// <summary>
        /// Set primary image for car (Admin only)
        /// </summary>
        [HttpPut("{carId}/images/{imageId}/primary")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPrimaryImage(int carId, int imageId)
        {
            var result = await _imageService.SetPrimaryImageAsync(carId, imageId);

            if (!result.Success)
                return NotFound(result);

            _logger.LogInformation("Set image {ImageId} as primary for car {CarId}", imageId, carId);
            return Ok(result);
        }
    }
}