using FCR.Dal.Classes;


namespace FCR.Dal.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        // Get bookings with related data
        Task<Booking?> GetBookingWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

        // Filter bookings
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetBookingsByCarIdAsync(int carId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetActiveBookingsAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(string userId, CancellationToken cancellationToken = default);

        // Availability checks
        Task<bool> HasBookingConflictAsync(int carId, DateTime pickupDate, DateTime returnDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetBookingConflictsAsync(int carId, DateTime pickupDate, DateTime returnDate, CancellationToken cancellationToken = default);

        // Statistics
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
        Task<int> GetTotalBookingsCountAsync(CancellationToken cancellationToken = default);
    }
}