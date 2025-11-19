using FCR.Bll.DTOs.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace FCR.Bll.DTOs
{
    public class CarResponseDto
    {
        // Basic Info
        public int CarId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Year { get; set; }

        // Pricing
        [Precision(10, 2)]
        public decimal DailyRate { get; set; }
        [Precision(10, 2)]
        public decimal? WeeklyRate { get; set; }
        [Precision(10, 2)]
        public decimal? MonthlyRate { get; set; }

        // Availability
        public bool IsAvailable { get; set; }
        public DateTime? AvailableFrom { get; set; }

        // Specifications
        public string Transmission { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public int Seats { get; set; }
        public string? Color { get; set; }
        public double Mileage { get; set; }
        public string? LicensePlate { get; set; }

        // Description
        public string? Description { get; set; }

        // Images
        public List<ImageResponseDto> Images { get; set; } = new List<ImageResponseDto>();
        public string? PrimaryImageUrl { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }

        // Metadata
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Statistics
        public int TotalBookings { get; set; }
        public decimal? AverageRating { get; set; }
    }
}