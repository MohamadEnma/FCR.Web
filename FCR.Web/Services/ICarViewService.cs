using FCR.Web.Services.Base;

namespace FCR.Web.Services
{
    /// <summary>
    /// Service for car viewing operations shared between controllers
    /// </summary>
    public interface ICarViewService
    {
        /// <summary>
        /// Get filtered and searched cars based on multiple criteria
        /// </summary>
        Task<List<CarResponseDto>> GetFilteredCarsAsync(
            string? searchTerm,
            string? category,
            string? transmission,
            string? fuelType,
            bool? isAvailable,
            double? minPrice,
            double? maxPrice,
            int? minSeats);

        /// <summary>
        /// Get car details by ID
        /// </summary>
        Task<CarResponseDto?> GetCarDetailsAsync(int carId);
    }
}
