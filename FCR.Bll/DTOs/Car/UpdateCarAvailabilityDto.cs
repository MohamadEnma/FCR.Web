using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Bll.DTOs.Car
{
    public class UpdateCarAvailabilityDto
    {
        [Required(ErrorMessage = "Availability status is required")]
        public bool IsAvailable { get; set; }
    }
}
