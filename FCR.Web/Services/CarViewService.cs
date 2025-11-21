using FCR.Web.Services.Base;

namespace FCR.Web.Services
{
    public class CarViewService : ICarViewService
    {
        private readonly IClient _apiClient;
        private readonly ILogger<CarViewService> _logger;

        public CarViewService(IClient apiClient, ILogger<CarViewService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<CarResponseDto>> GetFilteredCarsAsync(
            string? searchTerm,
            string? category,
            string? transmission,
            string? fuelType,
            bool? isAvailable,
            double? minPrice,
            double? maxPrice,
            int? minSeats)
        {
            try
            {
                IEnumerable<CarResponseDto> cars;

                // Apply API filters if any
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

                return cars.ToList();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading and filtering cars");
                return new List<CarResponseDto>();
            }
        }

        public async Task<CarResponseDto?> GetCarDetailsAsync(int carId)
        {
            try
            {
                var response = await _apiClient.CarsGET2Async(carId);
                return response?.Data;
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                _logger.LogWarning("Car with ID {CarId} not found", carId);
                return null;
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading car details for ID {CarId}", carId);
                return null;
            }
        }
    }
}
