using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FCR.Dal.Classes
{
    [Index(nameof(UserId))]
    [Index(nameof(CarId))]
    [Index(nameof(Status))]
    [Index(nameof(PickupDate))]
    [Index(nameof(IsCancelled))]
    [Index(nameof(IsDeleted))]
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled

        public bool IsCancelled { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime? CancellationDate { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [Required]
        [MaxLength(50)]
        public string BookingNumber { get; set; } = string.Empty;  // NEW

        public DateTime? CompletedDate { get; set; }  // NEW

        // Auto-generated timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CarId))]
        public virtual Car? Car { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}