using FCR.Dal.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Dal.Models
{
    public class HomeViewModel
    {
        public string? UserId { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public List<Car> AllCars { get; set; } = new List<Car>();

        public List<Car> AvailableCars => AllCars.Where(c => c.IsAvailable).ToList();
    }
}
