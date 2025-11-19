using System;
using System.ComponentModel.DataAnnotations;

namespace FCR.Bll.DTOs.Booking
{
    public class BookingCreateDto
    {
        [Required(ErrorMessage = "Car ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid car ID")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        [DataType(DataType.DateTime)]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Return date is required")]
        [DataType(DataType.DateTime)]
        public DateTime ReturnDate { get; set; }

        // Optional: Customer notes
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Validation: Return date must be after pickup date
        public bool IsValid()
        {
            return ReturnDate > PickupDate && PickupDate >= DateTime.UtcNow.Date;
        }
    }
}