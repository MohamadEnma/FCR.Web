using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Booking;
using FCR.Bll.Interfaces;
using FCR.Dal.Classes;
using FCR.Dal.Repositories.Interfaces;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(
            string userId,
            BookingCreateDto bookingDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate dates
                if (bookingDto.ReturnDate <= bookingDto.PickupDate)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Invalid dates",
                        "Return date must be after pickup date");
                }

                if (bookingDto.PickupDate < DateTime.UtcNow.Date)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Invalid dates",
                        "Pickup date cannot be in the past");
                }

                // Check if car exists and is available
                var car = await _unitOfWork.Cars.GetByIdAsync(bookingDto.CarId, cancellationToken);
                if (car == null || car.IsDeleted)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

                if (!car.IsAvailable)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Car unavailable",
                        "This car is not available for booking");
                }

                // Check for booking conflicts
                var hasConflict = await _unitOfWork.Bookings.HasBookingConflictAsync(
                    bookingDto.CarId,
                    bookingDto.PickupDate,
                    bookingDto.ReturnDate,
                    cancellationToken);

                if (hasConflict)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Booking conflict",
                        "Car is already booked for the selected dates");
                }

                // Calculate total price
                var totalDays = (bookingDto.ReturnDate - bookingDto.PickupDate).Days;
                var totalPrice = car.DailyRate * totalDays;

                // Apply weekly/monthly discounts if applicable
                if (totalDays >= 30 && car.MonthlyRate.HasValue)
                {
                    var months = totalDays / 30;
                    var remainingDays = totalDays % 30;
                    totalPrice = (car.MonthlyRate.Value * months) + (car.DailyRate * remainingDays);
                }
                else if (totalDays >= 7 && car.WeeklyRate.HasValue)
                {
                    var weeks = totalDays / 7;
                    var remainingDays = totalDays % 7;
                    totalPrice = (car.WeeklyRate.Value * weeks) + (car.DailyRate * remainingDays);
                }

                // Create booking
                var booking = new Booking
                {
                    CarId = bookingDto.CarId,
                    UserId = userId,
                    PickupDate = bookingDto.PickupDate,
                    ReturnDate = bookingDto.ReturnDate,
                    TotalPrice = totalPrice,
                    Status = "Pending",
                    IsCancelled = false,
                    BookingNumber = GenerateBookingNumber()
                };

                await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload booking with related data
                booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.BookingId, cancellationToken);

                var response = _mapper.Map<BookingResponseDto>(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    response,
                    "Booking created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Failed to create booking",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId, cancellationToken);
                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Booking not found",
                        "Invalid booking ID");
                }

                var response = _mapper.Map<BookingResponseDto>(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Failed to retrieve booking",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetUserBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByUserIdAsync(userId, cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetAllBookingsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetAllWithDetailsAsync(cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetBookingsByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByCarIdAsync(carId, cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetBookingsByStatusAsync(
            string status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByStatusAsync(status, cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetActiveBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetActiveBookingsAsync(userId, cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve active bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetCompletedBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByUserIdAsync(userId, cancellationToken);
                var completed = bookings.Where(b => b.Status == "Completed");
                var response = _mapper.Map<List<BookingResponseDto>>(completed);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve completed bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetUpcomingBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetUpcomingBookingsAsync(userId, cancellationToken);
                var response = _mapper.Map<List<BookingResponseDto>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<BookingResponseDto>>.ErrorResponse(
                    "Failed to retrieve upcoming bookings",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> UpdateBookingStatusAsync(
            int bookingId,
            string status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.ErrorResponse(
                        "Booking not found",
                        "Invalid booking ID");
                }

                booking.Status = status;

                if (status == "Completed")
                {
                    booking.CompletedDate = DateTime.UtcNow;
                }

                await _unitOfWork.Bookings.UpdateAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId, cancellationToken);
                var response = _mapper.Map<BookingResponseDto>(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    response,
                    "Booking status updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<BookingResponseDto>.ErrorResponse(
                    "Failed to update booking status",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            return await UpdateBookingStatusAsync(bookingId, "Confirmed", cancellationToken);
        }

        public async Task<ServiceResponse<BookingResponseDto>> CompleteBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            return await UpdateBookingStatusAsync(bookingId, "Completed", cancellationToken);
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(
            int bookingId,
            string userId,
            string? cancellationReason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
                if (booking == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Booking not found",
                        "Invalid booking ID");
                }

                // Verify user owns the booking
                if (booking.UserId != userId)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Unauthorized",
                        "You can only cancel your own bookings");
                }

                if (booking.IsCancelled)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Already cancelled",
                        "This booking has already been cancelled");
                }

                booking.IsCancelled = true;
                booking.CancellationDate = DateTime.UtcNow;
                booking.CancellationReason = cancellationReason;
                booking.Status = "Cancelled";

                await _unitOfWork.Bookings.UpdateAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Booking cancelled successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to cancel booking",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> IsCarAvailableAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hasConflict = await _unitOfWork.Bookings.HasBookingConflictAsync(
                    carId,
                    pickupDate,
                    returnDate,
                    cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(!hasConflict);
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to check availability",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<CarAvailabilityDto>> CheckCarAvailabilityAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var conflicts = await _unitOfWork.Bookings.GetBookingConflictsAsync(
                    carId,
                    pickupDate,
                    returnDate,
                    cancellationToken);

                var availabilityDto = new CarAvailabilityDto
                {
                    CarId = carId,
                    IsAvailable = !conflicts.Any(),
                    AvailableFrom = conflicts.Any() ? conflicts.Max(b => b.ReturnDate) : null,
                    Conflicts = conflicts.Select(b => new BookingConflict
                    {
                        BookingId = b.BookingId,
                        PickupDate = b.PickupDate,
                        ReturnDate = b.ReturnDate
                    }).ToList()
                };

                return ServiceResponse<CarAvailabilityDto>.SuccessResponse(availabilityDto);
            }
            catch (Exception ex)
            {
                return ServiceResponse<CarAvailabilityDto>.ErrorResponse(
                    "Failed to check car availability",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<decimal>> CalculateBookingPriceAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(carId, cancellationToken);
                if (car == null)
                {
                    return ServiceResponse<decimal>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }

                var totalDays = (returnDate - pickupDate).Days;
                var totalPrice = car.DailyRate * totalDays;

                // Apply discounts
                if (totalDays >= 30 && car.MonthlyRate.HasValue)
                {
                    var months = totalDays / 30;
                    var remainingDays = totalDays % 30;
                    totalPrice = (car.MonthlyRate.Value * months) + (car.DailyRate * remainingDays);
                }
                else if (totalDays >= 7 && car.WeeklyRate.HasValue)
                {
                    var weeks = totalDays / 7;
                    var remainingDays = totalDays % 7;
                    totalPrice = (car.WeeklyRate.Value * weeks) + (car.DailyRate * remainingDays);
                }

                return ServiceResponse<decimal>.SuccessResponse(
                    totalPrice,
                    $"Total price for {totalDays} days");
            }
            catch (Exception ex)
            {
                return ServiceResponse<decimal>.ErrorResponse(
                    "Failed to calculate price",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<int>> GetTotalBookingsCountAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetAllAsync(cancellationToken);
                var count = bookings.Count();

                return ServiceResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Failed to get bookings count",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<decimal>> GetTotalRevenueAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var revenue = await _unitOfWork.Bookings.GetTotalRevenueAsync(cancellationToken);

                return ServiceResponse<decimal>.SuccessResponse(
                    revenue,
                    $"Total revenue: ${revenue:N2}");
            }
            catch (Exception ex)
            {
                return ServiceResponse<decimal>.ErrorResponse(
                    "Failed to calculate revenue",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteBookingAsync(
    int bookingId,
    CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

                if (booking == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Booking not found",
                        "Invalid booking ID");
                }

                if (booking.IsDeleted)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Booking already deleted",
                        "This booking has already been deleted");
                }

                // Soft delete
                booking.IsDeleted = true;
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Booking deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete booking",
                    ex.Message);
            }
        }

        // Helper methods
        private string GenerateBookingNumber()
        {
            return $"BK-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

      
    }
}