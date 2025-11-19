using FCR.Dal.Classes;
using FCR.Dal.Data;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Implementation
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCarIdAsync(int carId, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Where(b => b.CarId == carId)
                .OrderByDescending(b => b.PickupDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Include(b => b.User)
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Where(b => b.UserId == userId &&
                           !b.IsCancelled &&
                           b.PickupDate <= now &&
                           b.ReturnDate >= now &&
                           (b.Status == "Confirmed" || b.Status == "Pending"))
                .OrderBy(b => b.ReturnDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _db.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Images.Where(i => i.IsPrimary))
                .Where(b => b.UserId == userId &&
                           !b.IsCancelled &&
                           b.PickupDate > now &&
                           (b.Status == "Confirmed" || b.Status == "Pending"))
                .OrderBy(b => b.PickupDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasBookingConflictAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .AnyAsync(b => b.CarId == carId &&
                              !b.IsCancelled &&
                              b.Status != "Cancelled" &&
                              b.Status != "Completed" &&
                              ((b.PickupDate >= pickupDate && b.PickupDate < returnDate) ||
                               (b.ReturnDate > pickupDate && b.ReturnDate <= returnDate) ||
                               (b.PickupDate <= pickupDate && b.ReturnDate >= returnDate)),
                          cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingConflictsAsync(
            int carId,
            DateTime pickupDate,
            DateTime returnDate,
            CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Where(b => b.CarId == carId &&
                           !b.IsCancelled &&
                           b.Status != "Cancelled" &&
                           b.Status != "Completed" &&
                           ((b.PickupDate >= pickupDate && b.PickupDate < returnDate) ||
                            (b.ReturnDate > pickupDate && b.ReturnDate <= returnDate) ||
                            (b.PickupDate <= pickupDate && b.ReturnDate >= returnDate)))
                .OrderBy(b => b.PickupDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Where(b => b.Status == "Completed" && !b.IsCancelled)
                .SumAsync(b => b.TotalPrice, cancellationToken);
        }

        public async Task<int> GetTotalBookingsCountAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Bookings.CountAsync(cancellationToken);
        }
    }
}