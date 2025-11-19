using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FCR.Bll.DTOs
{
    public class CarUpdateDto
    {
        // Basic Info
        [Required(ErrorMessage = "Brand is required")]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [MaxLength(30)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2030, ErrorMessage = "Year must be between 2000 and 2030")]
        public int Year { get; set; }

        // Pricing
        [Required(ErrorMessage = "Daily rate is required")]
        [Range(0.01, 10000, ErrorMessage = "Daily rate must be between 0.01 and 10000")]
        [Precision(10, 2)]
        public decimal DailyRate { get; set; }

        [Range(0.01, 50000, ErrorMessage = "Weekly rate must be positive")]
        [Precision(10, 2)]
        public decimal? WeeklyRate { get; set; }

        [Range(0.01, 200000, ErrorMessage = "Monthly rate must be positive")]
        [Precision(10, 2)]
        public decimal? MonthlyRate { get; set; }

        // Availability
        public bool IsAvailable { get; set; }

        // Specifications
        [Required(ErrorMessage = "Transmission type is required")]
        [MaxLength(20)]
        public string Transmission { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fuel type is required")]
        [MaxLength(20)]
        public string FuelType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(2, 9, ErrorMessage = "Seats must be between 2 and 9")]
        public int Seats { get; set; }

        [MaxLength(30)]
        public string? Color { get; set; }

        [Range(0, 500000, ErrorMessage = "Mileage must be between 0 and 500000")]
        public int Mileage { get; set; }

        [MaxLength(20)]
        public string? LicensePlate { get; set; }

        // Description
        [MaxLength(1000)]
        public string? Description { get; set; }

        // Images 
        public List<string>? ImageUrls { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }
}