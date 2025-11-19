using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FCR.Dal.Classes
{
    [Index(nameof(IsAvailable))]
    [Index(nameof(IsDeleted))]
    [Index(nameof(Category))]
    [Index(nameof(Brand))]
    [Index(nameof(LicensePlate))]
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

        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

        [JsonIgnore]
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? LicensePlate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        [Precision(10, 2)]
        public decimal? MonthlyRate { get; set; }
        [Precision(10, 2)]
        public decimal? WeeklyRate { get; set; }

        public double Mileage {  get; set; }


    }
}

