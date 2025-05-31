using FCR.Dal.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Dal.Models
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        [FutureDate(ErrorMessage = "Pickup date must be in the future")]
        public DateTime PickupDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Return date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        [DateGreaterThan("PickupDate", ErrorMessage = "Return date must be after pickup date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(2);

        [Display(Name = "Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice { get; set; }

        // Car details for display
        [Display(Name = "Car Brand")]
        public string? CarBrand { get; set; }

        [Display(Name = "Car Model")]
        public string? CarModel { get; set; }

        [Display(Name = "Daily Rate")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DailyRate { get; set; }

        public List<Image> CarImages { get; set; } = new List<Image>();

        [Display(Name = "Booking Date")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // For status display
        [Display(Name = "Status")]
        public string? Status { get; set; }
    }

    // Custom validation attribute for future dates
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is DateTime date && date >= DateTime.Today;
        }
    }

    // Custom validation attribute for date comparison
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (DateTime)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);

            return currentValue > comparisonValue
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}