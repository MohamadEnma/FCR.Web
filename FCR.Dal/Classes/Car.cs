using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FCR.Dal.Classes
{
    public class Car
    {
        [Key]
        public int CarId { get; set; }

        [Required, StringLength(100)]
        public string Brand { get; set; } = string.Empty; 

        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty; 

        [Required, StringLength(50)]
        public string Category { get; set; } = string.Empty; 

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        [Precision(10, 2)] 
        public decimal DailyRate { get; set; } 

        public bool IsAvailable { get; set; } = true;

        [StringLength(30)]
        public string Transmission { get; set; } = "Automatic"; 

        [StringLength(30)]
        public string FuelType { get; set; } = "Petrol"; // Diesel, Electric, Hybrid

        public int Seats { get; set; } = 5;

        public bool IsDeleted { get; set; } = false;

        public List<Image> Images { get; set; } = new List<Image>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

