using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using FCR.Dal.Data;
using FCR.Dal.Models;
using FCR.Dal.Classes;

namespace FCR.Web.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public BookingsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // ─────────────────────────────────────────────
        // ADMIN ACTIONS
        // ─────────────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return View(bookings);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await GetBookingWithIncludes(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await GetBookingWithIncludes(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                // Send cancellation email
                await SendCancellationEmail(booking, "Admin cancellation");
            }
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────
        // BOOKING FLOW
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Create(int carId)
        {
            var car = await _context.Cars
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.CarId == carId);

            if (car == null) return NotFound();

            var model = new BookingViewModel
            {
                CarId = car.CarId,
                CarBrand = car.Brand,
                CarModel = car.Model,
                DailyRate = car.DailyRate,
                CarImages = car.Images,
                PickupDate = DateTime.Today.AddDays(1),
                ReturnDate = DateTime.Today.AddDays(2)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate car images if validation fails
                model.CarImages = await _context.Images
                    .Where(i => i.CarId == model.CarId)
                    .ToListAsync();
                return View(model);
            }

            var validationResult = await ValidateBooking(model);
            if (validationResult != null) return validationResult;

            var booking = await CreateBooking(model);

            await SendConfirmationEmail(booking, model);

            return RedirectToAction(nameof(Confirmation), new { id = booking.BookingId });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await GetBookingWithIncludes(id);
            if (booking == null) return NotFound();

            // Verify current user owns this booking
            var userId = _userManager.GetUserId(User);
            if (booking.UserId != userId) return Forbid();

            return View(booking);
        }

        // ─────────────────────────────────────────────
        // USER BOOKINGS
        // ─────────────────────────────────────────────
        public async Task<IActionResult> MyBookings(string status = null)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.UserId == userId);

            query = FilterBookingsByStatus(query, status);

            var bookings = await query
                .OrderByDescending(b => b.PickupDate)
                .ToListAsync();

            ViewBag.CurrentFilter = status;
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (booking.UserId != userId) return Forbid();

            if (booking.PickupDate > DateTime.Now && !booking.IsCancelled)
            {
                booking.IsCancelled = true;
                booking.CancellationDate = DateTime.Now;
                await _context.SaveChangesAsync();

                await SendCancellationEmail(booking, "User cancellation");
            }

            return RedirectToAction(nameof(MyBookings));
        }

        // ─────────────────────────────────────────────
        // PRIVATE HELPER METHODS
        // ─────────────────────────────────────────────
        private async Task<Booking> GetBookingWithIncludes(int id)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        private async Task<IActionResult> ValidateBooking(BookingViewModel model)
        {
            if (model.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past");
                return View(model);
            }

            if (model.PickupDate >= model.ReturnDate)
            {
                ModelState.AddModelError("", "Return date must be after pickup date");
                return View(model);
            }

            var isAvailable = !await _context.Bookings
                .AnyAsync(b => b.CarId == model.CarId &&
                             !b.IsCancelled &&
                             b.PickupDate < model.ReturnDate &&
                             b.ReturnDate > model.PickupDate);

            if (!isAvailable)
            {
                ModelState.AddModelError("", "Car not available for selected dates");
                return View(model);
            }

            return null;
        }

        private async Task<Booking> CreateBooking(BookingViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            // Calculate total days (including partial days as full days)
            var totalDays = (int)Math.Ceiling((model.ReturnDate - model.PickupDate).TotalDays);
            var totalPrice = totalDays * model.DailyRate;

            var booking = new Booking
            {
                CarId = model.CarId,
                UserId = user.Id,
                PickupDate = model.PickupDate,
                ReturnDate = model.ReturnDate,
                TotalPrice = totalPrice,
                BookingDate = DateTime.Now,
                
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        private async Task SendConfirmationEmail(Booking booking, BookingViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var subject = $"Booking Confirmation #{booking.BookingId}";
                var message = $@"
                    <h2>Your booking is confirmed!</h2>
                    <p><strong>Vehicle:</strong> {model.CarBrand} {model.CarModel}</p>
                    <p><strong>Dates:</strong> {booking.PickupDate:d} to {booking.ReturnDate:d}</p>
                    <p><strong>Total:</strong> {booking.TotalPrice:C}</p>
                    <p><strong>Booking ID:</strong> {booking.BookingId}</p>
                    <p>You can view or cancel your booking at any time from your account.</p>";

                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }
            catch (Exception ex)
            {
                // Log email sending error
                Console.WriteLine($"Error sending confirmation email: {ex.Message}");
            }
        }

        private async Task SendCancellationEmail(Booking booking, string cancellationType)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(booking.UserId);
                if (user == null) return;

                var subject = $"Booking Cancelled #{booking.BookingId}";
                var message = $@"
                    <h2>Your booking has been cancelled</h2>
                    <p><strong>Booking ID:</strong> {booking.BookingId}</p>
                    <p><strong>Cancellation Date:</strong> {DateTime.Now:d}</p>
                    <p><strong>Reason:</strong> {cancellationType}</p>";

                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }
            catch (Exception ex)
            {
                // Log email sending error
                Console.WriteLine($"Error sending cancellation email: {ex.Message}");
            }
        }

        private IQueryable<Booking> FilterBookingsByStatus(IQueryable<Booking> query, string status)
        {
            var now = DateTime.Now;

            return status?.ToLower() switch
            {
                "upcoming" => query.Where(b => b.PickupDate > now && !b.IsCancelled),
                "active" => query.Where(b => b.PickupDate <= now && b.ReturnDate >= now && !b.IsCancelled),
                "completed" => query.Where(b => b.ReturnDate < now && !b.IsCancelled),
                "cancelled" => query.Where(b => b.IsCancelled),
                _ => query
            };
        }
    }
}