using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.Interfaces;
using FCR.Dal.Classes;
using FCR.Dal.Repositories.Interfaces;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Services
{
    public class CarService : ICarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CarService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<CarResponseDto>> CreateCarAsync(
            CarCreateDto carDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                
                var car = _mapper.Map<Car>(carDto);

                await _unitOfWork.Cars.AddAsync(car, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<CarResponseDto>(car);

                return ServiceResponse<CarResponseDto>.SuccessResponse(
                    response,
                    "Car created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<CarResponseDto>.ErrorResponse(
                    "Failed to create car",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<CarResponseDto>> UpdateCarAsync(
            int carId,
            CarUpdateDto carDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(carId, cancellationToken);
                if (car == null || car.IsDeleted)
                {
                    return ServiceResponse<CarResponseDto>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

               
                carDto.Adapt(car);

                await _unitOfWork.Cars.UpdateAsync(car, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<CarResponseDto>(car);

                return ServiceResponse<CarResponseDto>.SuccessResponse(
                    response,
                    "Car updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<CarResponseDto>.ErrorResponse(
                    "Failed to update car",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteCarAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(carId, cancellationToken);
                if (car == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

                car.IsDeleted = true;
                car.IsAvailable = false;

                await _unitOfWork.Cars.UpdateAsync(car, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Car deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete car",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<CarResponseDto>> GetCarByIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetCarWithImagesAsync(carId, cancellationToken);
                if (car == null || car.IsDeleted)
                {
                    return ServiceResponse<CarResponseDto>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

                var response = _mapper.Map<CarResponseDto>(car);

                return ServiceResponse<CarResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<CarResponseDto>.ErrorResponse(
                    "Failed to retrieve car",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetAllCarsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAllWithImagesAsync(cancellationToken);
                var activeCars = cars.Where(c => !c.IsDeleted);

                var response = _mapper.Map<IEnumerable<CarResponseDto>>(activeCars);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve cars",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetAvailableCarsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAvailableCarsAsync(cancellationToken);
                var response = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve available cars",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetCarsByBrandAsync(
            string brand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetCarsByBrandAsync(brand, cancellationToken);
                var response = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve cars by brand",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> GetCarsByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetCarsByCategoryAsync(category, cancellationToken);
                var response = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve cars by category",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> SearchCarsAsync(
            string keyword,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.SearchCarsAsync(keyword, cancellationToken);
                var response = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to search cars",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<CarResponseDto>>> FilterCarsAsync(
            string? category,
            string? transmission,
            string? fuelType,
            int? minSeats,
            decimal? maxDailyRate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAllWithImagesAsync(cancellationToken);

                var filtered = cars.Where(c => !c.IsDeleted && c.IsAvailable);

                if (!string.IsNullOrEmpty(category))
                    filtered = filtered.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(transmission))
                    filtered = filtered.Where(c => c.Transmission.Equals(transmission, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(fuelType))
                    filtered = filtered.Where(c => c.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase));

                if (minSeats.HasValue)
                    filtered = filtered.Where(c => c.Seats >= minSeats.Value);

                if (maxDailyRate.HasValue)
                    filtered = filtered.Where(c => c.DailyRate <= maxDailyRate.Value);

                var response = _mapper.Map<IEnumerable<CarResponseDto>>(filtered);

                return ServiceResponse<IEnumerable<CarResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<CarResponseDto>>.ErrorResponse(
                    "Failed to filter cars",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> UpdateCarAvailabilityAsync(
            int carId,
            bool isAvailable,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(carId, cancellationToken);
                if (car == null || car.IsDeleted)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

                car.IsAvailable = isAvailable;

                await _unitOfWork.Cars.UpdateAsync(car, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    $"Car availability updated to {isAvailable}");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to update car availability",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<PagedResult<CarResponseDto>>> GetAllCarsPaginatedAsync(
           PaginationParams paginationParams,
           CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAllWithImagesAsync(cancellationToken);
                var activeCars = cars.Where(c => !c.IsDeleted);

                var carDtos = _mapper.Map<IEnumerable<CarResponseDto>>(activeCars);

                var pagedResult = carDtos.ToPagedResult(
                    paginationParams.PageNumber,
                    paginationParams.PageSize);

                return ServiceResponse<PagedResult<CarResponseDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResult<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve cars",
                    ex.Message);
            }
        }

        //Paginated GetAvailableCars
        public async Task<ServiceResponse<PagedResult<CarResponseDto>>> GetAvailableCarsPaginatedAsync(
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAvailableCarsAsync(cancellationToken);
                var carDtos = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

                var pagedResult = carDtos.ToPagedResult(
                    paginationParams.PageNumber,
                    paginationParams.PageSize);

                return ServiceResponse<PagedResult<CarResponseDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResult<CarResponseDto>>.ErrorResponse(
                    "Failed to retrieve available cars",
                    ex.Message);
            }
        }

        //  Paginated FilterCars
        public async Task<ServiceResponse<PagedResult<CarResponseDto>>> FilterCarsPaginatedAsync(
            string? category,
            string? transmission,
            string? fuelType,
            int? minSeats,
            decimal? maxDailyRate,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAllWithImagesAsync(cancellationToken);

                var filtered = cars.Where(c => !c.IsDeleted && c.IsAvailable);

                if (!string.IsNullOrEmpty(category))
                    filtered = filtered.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(transmission))
                    filtered = filtered.Where(c => c.Transmission.Equals(transmission, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(fuelType))
                    filtered = filtered.Where(c => c.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase));

                if (minSeats.HasValue)
                    filtered = filtered.Where(c => c.Seats >= minSeats.Value);

                if (maxDailyRate.HasValue)
                    filtered = filtered.Where(c => c.DailyRate <= maxDailyRate.Value);

                var carDtos = _mapper.Map<IEnumerable<CarResponseDto>>(filtered);

                var pagedResult = carDtos.ToPagedResult(
                    paginationParams.PageNumber,
                    paginationParams.PageSize);

                return ServiceResponse<PagedResult<CarResponseDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResult<CarResponseDto>>.ErrorResponse(
                    "Failed to filter cars",
                    ex.Message);
            }
        }

    }
}