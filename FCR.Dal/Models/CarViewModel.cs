using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FCR.Dal.Models
{
    public class CarViewModel
    {
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
        [Range(0.01, 999999.99)]
        public decimal DailyRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(30)]
        public string Transmission { get; set; } = "Automatic";

        [StringLength(30)]
        public string FuelType { get; set; } = "Petrol";

        [Range(1, 20)]
        public int Seats { get; set; } = 5;

        // Optional: make this hidden or managed internally
        public bool IsDeleted { get; set; } = false;

        // Used for manual entry of image URLs
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
