using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Bll.DTOs.Booking
{
    public class CheckAvailabilityDto
    {
        [Required(ErrorMessage = "Car ID is required")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Return date is required")]
        public DateTime ReturnDate { get; set; }
    }
}
