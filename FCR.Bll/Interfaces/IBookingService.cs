using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Interfaces
{
    public interface IBookingService  // CHANGED: internal → public
    {
        // Create Booking
        Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(
            string userId,
            BookingCreateDto bookingDto,
            CancellationToken cancellationToken = default);

        // Get Bookings
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetUserBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetAllBookingsAsync(
            CancellationToken cancellationToken = default);  // Admin only

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetBookingsByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default);

        // Filter Bookings
        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetBookingsByStatusAsync(
            string status,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetActiveBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetCompletedBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<BookingResponseDto>>> GetUpcomingBookingsAsync(
            string userId,
            CancellationToken cancellationToken = default);

        // Update Booking
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingStatusAsync(
            int bookingId,
            string status,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<BookingResponseDto>> CompleteBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        // Cancel Booking
        Task<ServiceResponse<bool>> CancelBookingAsync(
            int bookingId,
            string userId,
            string? cancellationReason,
            CancellationToken cancellationToken = default);

        // Check Availability
        Task<ServiceResponse<bool>> IsCarAvailableAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<CarAvailabilityDto>> CheckCarAvailabilityAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default);

        // Calculate Price
        Task<ServiceResponse<decimal>> CalculateBookingPriceAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default);

        // Statistics (Admin)
        Task<ServiceResponse<int>> GetTotalBookingsCountAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<decimal>> GetTotalRevenueAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> DeleteBookingAsync(
    int bookingId,
    CancellationToken cancellationToken = default);
    }
}