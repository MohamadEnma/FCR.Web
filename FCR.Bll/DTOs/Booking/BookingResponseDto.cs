using System;

namespace FCR.Bll.DTOs.Booking
{
    public class BookingResponseDto
    {
        // Booking Info
        public int BookingId { get; set; }  
        public string BookingNumber { get; set; } = string.Empty; 
        public DateTime BookingDate { get; set; }  

        // Car Info
        public int CarId { get; set; }
        public string CarBrand { get; set; } = string.Empty; 
        public string CarModel { get; set; } = string.Empty;  
        public int CarYear { get; set; }  
        public string? CarImageUrl { get; set; }  

        // User Info
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty; 
        public string UserEmail { get; set; } = string.Empty;  

        // Dates
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int TotalDays { get; set; }  

        // Pricing
        public decimal DailyRate { get; set; } 
        public decimal TotalPrice { get; set; }

        // Status
        public string Status { get; set; } = string.Empty;  
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }  

      
        public DateTime CreatedAt { get; set; }  
        public DateTime? CompletedDate { get; set; }  
    }
}