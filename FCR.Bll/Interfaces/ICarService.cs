using FCR.Bll.Common;
using FCR.Bll.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Interfaces
{
    public interface ICarService
    {
        // Create & Update
        Task<ServiceResponse<CarResponseDto>> CreateCarAsync(
            CarCreateDto carDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<CarResponseDto>> UpdateCarAsync(
            int carId,
            CarUpdateDto carDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> DeleteCarAsync(
            int carId,
            CancellationToken cancellationToken = default);

        // Get Cars
        Task<ServiceResponse<CarResponseDto>> GetCarByIdAsync(
            int carId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetAllCarsAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetAvailableCarsAsync(
            CancellationToken cancellationToken = default);

        // Filter & Search
        Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetCarsByBrandAsync(
            string brand,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetCarsByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<CarResponseDto>>> SearchCarsAsync(
            string keyword,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<CarResponseDto>>> FilterCarsAsync(
            string? category,
            string? transmission,
            string? fuelType,
            int? minSeats,
            decimal? maxDailyRate,
            CancellationToken cancellationToken = default);

        // Availability
        Task<ServiceResponse<bool>> UpdateCarAvailabilityAsync(
            int carId,
            bool isAvailable,
            CancellationToken cancellationToken = default);


        Task<ServiceResponse<PagedResult<CarResponseDto>>> GetAllCarsPaginatedAsync(
    PaginationParams paginationParams,
    CancellationToken cancellationToken = default);

        Task<ServiceResponse<PagedResult<CarResponseDto>>> GetAvailableCarsPaginatedAsync(
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<PagedResult<CarResponseDto>>> FilterCarsPaginatedAsync(
            string? category,
            string? transmission,
            string? fuelType,
            int? minSeats,
            decimal? maxDailyRate,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default);


    }
}